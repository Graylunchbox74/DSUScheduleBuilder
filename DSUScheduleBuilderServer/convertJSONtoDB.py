import sqlite3
import json

db = sqlite3.connect("userDatabase.db")
c  = db.cursor()

data = None
with open("../Scraping/totalData.json", "r") as f:
    data = json.load(f)

for term in data:
    if term != "Teachers":
        for collegeKey, college in data[term].items():
            print(collegeKey)
            for couresName, course in college.items():
                for section in course:
                    for sectionID, cd in section.items():
                        c.execute('SELECT * FROM availableCourses WHERE sectionID=? AND term=?', (sectionID, term))
                        res = c.fetchone()
                        if res:
                            cmd = '''
                            UPDATE
                            availableCourses
                            SET sectionID=?
                            , open=?
                            , academicLevel=?
                            , courseID=?
                            , description=?
                            , courseName=?
                            , startDate=?
                            , endDate=?
                            , location=?
                            , meetingInformation=?
                            , supplies=?
                            , credits=?
                            , slotsAvailable=?
                            , slotsCapacity=?
                            , slotsWaitlist=?
                            , timeStart=?
                            , timeEnd=?
                            , professorEmails=?
                            , prereqNonCourse=?
                            , recConcurrentCourses=?
                            , reqConcurrentCourses=?
                            , prereqCoursesAnd=?
                            , prereqCoursesOR=?
                            , instructionalMethods=?
                            , daysOfWeek=?
                            , term=?
                            WHERE
                            sectionID=? AND term=?
                            '''
                        else:
                            cmd = '''
                            INSERT INTO
                            availableCourses
                            ( sectionID
                            , open
                            , academicLevel
                            , courseID
                            , description
                            , courseName
                            , startDate
                            , endDate
                            , location
                            , meetingInformation
                            , supplies
                            , credits
                            , slotsAvailable
                            , slotsCapacity
                            , slotsWaitlist
                            , timeStart
                            , timeEnd
                            , professorEmails
                            , prereqNonCourse
                            , recConcurrentCourses
                            , reqConcurrentCourses
                            , prereqCoursesAnd
                            , prereqCoursesOR
                            , instructionalMethods
                            , daysOfWeek
                            , term)
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                            '''

                        daysOfWeek = []
                        daysConvert = {
                            "Monday": "mon",
                            "Tuesday": "tues",
                            "Wednesday": "wed",
                            "Thursday": "thur",
                            "Friday": "fri"
                        }

                        for day, shortName in daysConvert.items():
                            if day in cd["MeetingInformation"]:
                                daysOfWeek.append(shortName) 

                        if res:
                            param = (sectionID,
                            cd["Open"],
                            cd["AcademicLevel"],
                            cd["CourseCode"],
                            cd["CourseDescription"],
                            cd["CourseName"],
                            cd["DateStart"],
                            cd["DateEnd"],
                            cd["Location"],
                            cd["MeetingInformation"], 
                            cd["Supplies"],
                            cd["Credits"],
                            cd["SlotsAvailable"],
                            cd["SlotsCapacity"],
                            cd["SlotsWaitlist"],
                            cd["TimeStart"],
                            cd["TimeEnd"],
                            "|" + ("|".join(cd["ProfessorEmails"])) + "|",
                            cd["PrereqNonCourse"],
                            "|" + "|".join(cd["RecConcurrentCourses"]) + "|",
                            "|" + "|".join(cd["ReqConcurrentCourses"]) + "|",
                            "|" + "|".join(cd["PrereqCourses"]["and"]) + "|",
                            "|" + "|".join(cd["PrereqCourses"]["or"]) + "|",
                            "",
                            "|" + "|".join(daysOfWeek) + "|",
                            term, sectionID, term)
                        else:
                            param = (sectionID,
                            cd["Open"],
                            cd["AcademicLevel"],
                            cd["CourseCode"],
                            cd["CourseDescription"],
                            cd["CourseName"],
                            cd["DateStart"],
                            cd["DateEnd"],
                            cd["Location"],
                            cd["MeetingInformation"], 
                            cd["Supplies"],
                            cd["Credits"],
                            cd["SlotsAvailable"],
                            cd["SlotsCapacity"],
                            cd["SlotsWaitlist"],
                            cd["TimeStart"],
                            cd["TimeEnd"],
                            "|" + ("|".join(cd["ProfessorEmails"])) + "|",
                            cd["PrereqNonCourse"],
                            "|" + "|".join(cd["RecConcurrentCourses"]) + "|",
                            "|" + "|".join(cd["ReqConcurrentCourses"]) + "|",
                            "|" + "|".join(cd["PrereqCourses"]["and"]) + "|",
                            "|" + "|".join(cd["PrereqCourses"]["or"]) + "|",
                            "",
                            "|" + "|".join(daysOfWeek) + "|",
                            term)

                        db.execute(cmd, param)
    else:
        for teacherEmail, teacher in data[term].items():
            c.execute('SELECT * FROM teachers WHERE email=?', (teacherEmail,))
            res = c.fetchone()

            if res:
                cmd = '''
                UPDATE
                teachers
                SET 
                name=?, email=?, phone=?
                '''
            else:
                cmd = '''
                INSERT INTO
                teachers
                ( name, email, phone )
                VALUES (?, ?, ?)
                '''

            param = (
                teacher["Name"],
                teacher["Email"],
                teacher["Phone"]
            )
            db.execute(cmd, param)
db.commit()
db.close()