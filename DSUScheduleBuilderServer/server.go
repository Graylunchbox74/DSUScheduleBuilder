package main

import (
	"database/sql"
	"errors"
	"fmt"
	"net/http"

	"github.com/gin-contrib/static"
	"github.com/gin-gonic/gin"
	_ "github.com/mattn/go-sqlite3"
	"golang.org/x/crypto/bcrypt"
)

//make the database global, db = pointer to a database
var db *sql.DB
var errorChannel chan locationalError

//holds the information for a single course being/has been offered
type course struct {
	UserID    int `json:"uid"`
	StartTime int `json:"startTime"`
	EndTime   int `json:"endTime"`
	Credits   int `json:"credits"`

	ClassID   string `json:"classID"`
	ClassName string `json:"className"`
	Location  string `json:"location"`
	Teacher   string `json:"teacher"`
	StartDate string `json:"startDate"`
	EndDate   string `json:"endDate"`
}

type locationalError struct {
	Error                 error
	Location, Sublocation string
}

//User holds the information for a single user
type User struct {
	Email  string `json:"email"`
	Fname  string `json:"fname"`
	Lname  string `json:"lname"`
	Majors string `json:"majors"`
	Minors string `json:"minors"`
	UID    int    `json:"uid"`
}

//errorStruct holds an error
type errorStruct struct {
	ErrorStatusCode int    `json:"errorCode"`
	ErrorLogMessage string `json:"errorMessage"`
}

func checkLogError(location, sublocation string, err error) {
	if err != nil {
		logError(location, sublocation, err)
	}
}

func logError(location, sublocation string, err error) {
	errorChannel <- locationalError{err, location, sublocation}
}

func errorDrain() {
	var lErr locationalError
	for {
		select {
		case lErr = <-errorChannel:
			fmt.Println(lErr.Location, lErr.Sublocation, lErr.Error)
			//Handle Error Logging Here
		}
	}
}

func hashPassword(password string) (string, error) {
	bytes, err := bcrypt.GenerateFromPassword([]byte(password), 14)
	return string(bytes), err
}

func checkPasswordHash(password, hash string) bool {
	err := bcrypt.CompareHashAndPassword([]byte(hash), []byte(password))
	return err == nil
}

func doesUserExistWithField(field, value interface{}) bool {
	err := db.QueryRow(fmt.Sprintf("SELECT %s FROM user WHERE %s=$1", field, field), value).Scan(&value)
	return err != nil
}

func createErrorStruct(code int, location, sublocation string, err error) errorStruct {
	go logError(location, sublocation, err)
	return errorStruct{code, fmt.Sprintf("%s, %s: %s", location, sublocation, err.Error())}
}

func getUserID(name string) int {
	var uid int
	err := db.QueryRow("SELECT id FROM user WHERE name=$1", name).Scan(&uid)
	checkLogError("getUserID", "1", err)
	return uid
}

//user database functions
//create new user given name, password, string, by inputing into database
func newUser(fname, lname, major, email, minor string, password string) (int, error) {
	location := "newUser"

	//the user does not currently exist in the database with the same email
	if doesUserExistWithField("email", email) {
		return 6, errors.New("User with that email already exists in database")
	}

	//hash the password to store it
	maxAttempts, numAttempts := 10, 0
	var err error
	for err = nil; err != nil && numAttempts <= maxAttempts; {
		password, err = hashPassword(password)
		numAttempts++
	}

	if err != nil {
		go logError(location, "2", err)
		return 1, err
	}

	_, err = db.Exec(`
		INSERT INTO users 
		values($1,$2,$3,$4,$5)`,
		fname, lname, major,
		email, password,
	)

	if err != nil {
		go logError(location, "1", err)
		return 1, errors.New("Error inserting new user into database")
	}

	return 200, nil
}

//delete a user given a user struct from the database
//NEEDS TO BE REFACTORED!
func deleteUser(user User) (int, error) {
	location := "deleteUser"
	var err error
	if doesUserExistWithField("id", user.UID) {
		var uid int
		err := db.QueryRow("SELECT id FROM user WHERE id=$1", user.UID).Scan(&uid)
		checkLogError(location, "Check if user exists before deleting", err)
		return 500, err
	}
	//delete the user from the user table
	_, err = db.Exec("DELETE FROM user WHERE id=$1", user.UID)
	checkLogError(location, "Delete user information from user table", err)
	if err == nil {
		_, err = db.Exec("DELETE FROM PreviousClasses WHERE userID=$1", user.UID)
		checkLogError(location, "Delete user information from PreviousClasses", err)
		if err == nil {
			_, err = db.Exec("DELETE FROM EnrolledClasses WHERE userID=$1", user.UID)
			checkLogError(location, "Delete user information from EnrolledClasses", err)
			if err == nil {
				return 200, err
			}
		}
	}
	return 500, err
}

//update information in the user table for a user KEYWORD = the column you want to change and NEWVALUE = the value to change to
func updateUser(user User, keyword, newValue string) (int, error) {
	location := "updateUser"
	var err error
	if newValue != "password" {
		_, err = db.Exec("UPDATE user SET $1=$2 WHERE id=$3", keyword, newValue, user.UID)
		checkLogError(location, "Update user information that is not password", err)
	} else {
		newValue, _ := hashPassword(newValue)
		_, err := db.Exec("UPDATE user SET $1=$2 WHERE id=$3", keyword, newValue, user.UID)
		checkLogError(location, "Update user password", err)
	}
	if err == nil {
		return 200, err
	}
	return 500, err
}

//given the name of a user return a structure with the user information
func getUser(id int) (User, int, error) {
	location := "getUser"
	var user User
	err := db.QueryRow("SELECT uid,fname,lname,email,minors,majors FROM USERS WHERE uid=$1", id).Scan(&user.UID, &user.Fname, &user.Lname, &user.Email, &user.Minors, &user.Majors)
	checkLogError(location, "Selecting the user by name", err)
	if err == nil {
		return user, 200, err
	}
	return user, 4, err
}

//enrolled class database functions
func addEnrolledClass(class course) (int, error) {
	//make sure this class does not exist for the user with this id already, else skip
	location := "addEnrolledClass"
	var tmp int
	var err error
	tmp = -1
	err = db.QueryRow("SELECT userID FROM EnrolledClasses WHERE userID=$1 AND classID=$2", class.UserID, class.ClassID).Scan(&tmp)
	checkLogError(location, "Selecting from database to see if it already exists", err)
	if tmp == -1 && err == nil {
		_, err = db.Exec("INSERT INTO EnrolledClasses (userID, classID, className, teacher, location, startTime, endTime, startDate, endDate, credits) values($1,$2,$3,$4,$5,$6,$7,$8,$9,$10)", class.UserID, class.ClassID, class.ClassName, class.Teacher, class.Location, class.StartTime, class.EndTime, class.StartDate, class.EndDate, class.Credits)
		checkLogError(location, "Insert the enrolled class in the database", err)
		if err == nil {
			return 200, err
		}
	} else if tmp != -1 {
		return 501, err
	}
	return 500, err
}

func deleteEnrolledClass(class course) (int, error) {
	location := "deleteEnrolledClass"
	_, err := db.Exec("DELETE FROM EnrolledClasses WHERE userID=$1 AND classID=$2", class.UserID, class.ClassID)
	checkLogError(location, "Delete enrolled class from database", err)
	if err == nil {
		return 200, err
	}
	return 500, err
}

//updates an entry in the enrolledclasses table, although we cannot allow to change the userID
func updateEnrolledClass(class course, keyword, newValue string) (int, error) {
	location := "updateEnrolledClass"
	var err error
	if keyword != "classID" {
		_, err = db.Exec("UPDATE EnrolledClasses SET $1=$2 WHERE userID=$3 AND classID=$4", keyword, newValue, class.UserID, class.ClassID)
		checkLogError(location, "Updating in EnrolledClass something that is not classID", err)
	} else {
		_, err = db.Exec("UPDATE EnrolledClasses SET $1=$2 WHERE userID=$3 AND className=$4", keyword, newValue, class.UserID, class.ClassName)
		checkLogError(location, "Updating in EnrolledClass the classID", err)
	}
	if err == nil {
		return 200, err
	}
	return 500, err
}

func getEnrolledClass(uid int, classID string) (course, int, error) {
	var class course
	location := "getEnrolledClass"
	err := db.QueryRow("SELECT FROM EnrolledClasses WHERE userID=$1 AND classID=$2", uid, classID).Scan(&class.UserID, &class.ClassID, &class.ClassName, &class.Teacher, &class.Location, &class.StartTime, &class.EndTime, &class.StartDate, &class.EndDate, &class.Credits)
	checkLogError(location, "Selecting from the enrolledClasses table", err)
	if err == nil {
		return class, 200, err
	}
	return class, 500, err
}

//previous class database functions
func addPreviousClass(class course) (int, error) {
	//make sure this class does not exist for the user with this id already, else skip
	var tmp int
	location := "addPreviousClass"
	var err error
	tmp = -1
	err = db.QueryRow("SELECT userID FROM PreviousClasses WHERE userID=$1 AND classID=$2", class.UserID, class.ClassID).Scan(&tmp)
	checkLogError(location, "Checking if class already exists", err)
	if tmp == -1 && err == nil {
		_, err = db.Exec("INSERT INTO PreviousClasses (userID, classID, className, teacher, startTime, endTime, startDate, endDate, credits) values($1,$2,$3,$4,$5,$6,$7,$8,$9)", class.UserID, class.ClassID, class.ClassName, class.Teacher, class.StartTime, class.EndTime, class.StartDate, class.EndDate, class.Credits)
		checkLogError(location, "Inserting the new class into database", err)
		if err == nil {
			return 200, err
		}
	} else if tmp != -1 {
		return 501, err
	}
	return 500, err
}

func deletePreviousClass(class course) (int, error) {
	location := "deletePreviousClass"
	var err error
	_, err = db.Exec("DELETE FROM PreviousClasses WHERE userID=$1 AND classID=$2", class.UserID, class.ClassID)
	checkLogError(location, "Deleted class from database", err)
	if err == nil {
		return 200, err
	}
	return 500, err
}

//updates an entry in the enrolledclasses table, although we cannot allow to change the userID
func updatePreviousClass(class course, keyword, newValue string) (int, error) {
	location := "updatePreviousClass"
	var err error
	if keyword != "classID" {
		_, err := db.Exec("UPDATE PreviousClasses SET $1=$2 WHERE userID=$3 AND classID=$4", keyword, newValue, class.UserID, class.ClassID)
		checkLogError(location, "Updated something that is not classID in PreviousClasses", err)
	} else {
		_, err := db.Exec("UPDATE PreviousClasses SET $1=$2 WHERE userID=$3 AND className=$4", keyword, newValue, class.UserID, class.ClassName)
		checkLogError(location, "Updated classID in PreviousClasses", err)
	}
	if err == nil {
		return 200, err
	}
	return 500, err
}

func getPreviousClasses(uid int) ([]course, int, error) {
	location := "getPreviousClasses"
	var classes []course
	var class course

	rows, err := db.Query(`
		SELECT *
		FROM PreviousClasses 
		WHERE userID=$1`, uid,
	)

	defer rows.Close()

	for rows.Next() {
		class = course{}

		err = rows.Scan(
			&class.UserID, &class.ClassID, &class.ClassName, &class.Teacher,
			&class.StartTime, &class.EndTime, &class.StartDate, &class.EndDate, &class.Credits,
		)

		if err != nil {
			go logError(location, "1", err)
			return classes, 5, err
		}

		classes = append(classes, class)
	}

	err = rows.Err()

	if err != nil {
		go logError(location, "2", err)
		return classes, 5, err
	}

	return classes, 200, nil
}

func init() {
	errorChannel = make(chan locationalError)

	var err error

	db, err = sql.Open("sqlite3", "./userDatabase.db?_busy_timeout=5000")
	if err != nil {
		panic(err)
	}
}

func main() {
	errorChannel = make(chan locationalError)
	go errorDrain()
	r := gin.Default()

	r.GET("/", func(c *gin.Context) { http.ServeFile(c.Writer, c.Request, "./index.html") })
	r.GET("/static/css/:fi", static.Serve("/static/css", static.LocalFile("static/css/", true)))
	r.GET("/static/img/:fi", static.Serve("/static/img", static.LocalFile("static/img/", true)))
	r.GET("/static/js/:fi", static.Serve("/static/js", static.LocalFile("static/js/", true)))
	r.GET("/static/custom/:fi", static.Serve("/static/custom", static.LocalFile("static/custom/", true)))

	api := r.Group("/api")
	{
		api.GET("/something", func(c *gin.Context) {
			c.JSON(200, gin.H{"msg": ""})
		})

		api.GET("/user/:uuid", func(c *gin.Context) {
			uuid := c.Param("uuid")
			var userID int
			err := db.QueryRow("SELECT uid FROM USER_SESSIONS WHERE uuid=$1", uuid).Scan(&userID)

			if err == sql.ErrNoRows {
				currentError := createErrorStruct(2, c.Request.URL.String(), "3", err)
				c.JSON(500, currentError)
				return
			}

			if err != nil {
				currentError := createErrorStruct(3, c.Request.URL.String(), "1", err)
				c.JSON(500, currentError)
				return
			}

			var user User
			user, errno, err := getUser(userID)

			if err != nil {
				currentError := createErrorStruct(errno, c.Request.URL.String(), "2", err)
				c.JSON(500, currentError)
				return
			}

			c.JSON(200, user)
		})

		courses := api.Group("courses")
		{

			courses.GET("/previous/:uuid", func(c *gin.Context) {
				uuid := c.Param("uuid")
				var userID int
				err := db.QueryRow("SELECT uid FROM USER_SESSIONS WHERE uuid=$1", uuid).Scan(&userID)

				if err == sql.ErrNoRows {
					currentError := createErrorStruct(2, c.Request.URL.String(), "3", err)
					c.JSON(500, currentError)
					return
				}

				if err != nil {
					currentError := createErrorStruct(3, c.Request.URL.String(), "1", err)
					c.JSON(500, currentError)
					return
				}

				var classes []course
				classes, errno, err := getPreviousClasses(userID)

				if err != nil {
					currentError := createErrorStruct(errno, c.Request.URL.String(), "2", err)
					c.JSON(500, currentError)
					return
				}

				c.JSON(200, classes)
			})

			// courses.GET("/user/:uuid", func(c *gin.Context) {
			// 	uuid := c.Param("uuid")
			// 	var userID int
			// 	err := db.QueryRow("SELECT uid FROM USER_SESSIONS WHERE uuid=$1", uuid).Scan(&userID)

			// 	if err == sql.ErrNoRows {
			// 		currentError := createErrorStruct(2, c.Request.URL.String(), "3", err)
			// 		c.JSON(500, currentError)
			// 		return
			// 	}

			// 	if err != nil {
			// 		currentError := createErrorStruct(3, c.Request.URL.String(), "1", err)
			// 		c.JSON(500, currentError)
			// 		return
			// 	}

			// 	//var allCources []course

			//select all courses that are available to register for

			// })

		}
	}
	r.Run(":4200")
}
