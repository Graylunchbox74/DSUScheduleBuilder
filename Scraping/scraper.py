from re import match
from splinter import Browser
from time import sleep
import glob, json, os, re, requests, sys

class Course:
    Open                 = False

    AcademicLevel        = ""
    CourseCode           = ""
    CourseDescription    = ""
    CourseName           = ""
    DateStart            = ""
    DateEnd              = ""
    Location             = ""
    MeetingInformation   = ""
    Supplies             = ""
    
    Credits              = 0
    SlotsAvailable       = 0
    SlotsCapacity        = 0
    SlotsWaitlist        = 0
    TimeEnd              = 0
    TimeStart            = 0

    ProfessorEmails      = []
    PrereqNonCourse      = []
    RecConcurrentCourses = []
    ReqConcurrentCourses = []

    PrereqCourses        = {}
    InstructionalMethods = {}

    def __init__(self):
        self.Open                 = False

        self.AcademicLevel        = ""
        self.CourseCode           = ""
        self.CourseDescription    = ""
        self.CourseName           = ""
        self.DateStart            = ""
        self.DateEnd              = ""
        self.Location             = ""
        self.MeetingInformation   = ""
        self.Supplies             = ""
        
        self.Credits              = 0
        self.SlotsAvailable       = 0
        self.SlotsCapacity        = 0
        self.SlotsWaitlist        = 0
        self.TimeEnd              = 0
        self.TimeStart            = 0

        self.ProfessorEmails      = []
        self.PrereqNonCourse      = []
        self.RecConcurrentCourses = []
        self.ReqConcurrentCourses = []

        self.PrereqCourses        = {}
        self.InstructionalMethods = {}


class Teacher:
    Email = ""
    Name  = ""
    Phone = ""
    def __init__(self, e, n, p):
        self.Email = e
        self.Name = n
        self.Phone = p


""" JSON Model for Data
    {
        "Teachers": {"{Teacher Email}": {Teacher Object}},
        "TotalTime": "{Total time it took for the data for all semesters to be collected}",
        "{Semester}": {
            "Time": "{Time it took for the data to be collected for the given semester}",
            "Courses": {
                "{Subject}": {"Course Code": [{"{Section Codes}": {Course Object}}]}
            }
        }        
    }
"""

with open(glob.glob("*_config.json")[0]) as fi:
    config = json.load(fi)
executable_path = {'executable_path': config['browser']}
b = Browser(config['type'], headless=config['headless'], **executable_path)
teachers = {}
totalData = {"Teachers": {}}

def main():    
    initToQuery()
    semesters = getSemesters()
    subjects  = getSubjects()
    for semester in semesters:
        subjectCourses = {}
        for subject in subjects:
            unsuccessfulRun = True
            run = 0
            while unsuccessfulRun:
                run += 1
                print("Run", run, "For", subject)
                try:
                    handleExtraExits()
                    if b.is_element_present_by_text("Section Selection Results"):
                        unsuccessfulBackOut = True
                        while unsuccessfulBackOut:
                            try:
                                b.find_by_text("Go back").first.click()
                                unsuccessfulBackOut = False
                            except Exception as e:
                                print("Failed to go back: ", e)
                        while b.is_element_not_present_by_id("VAR1", 1):
                            pass
                    elif not b.is_element_present_by_text("Search for Class Sections"):
                        b.execute_script("window.location.reload()")
                        while b.is_element_not_present_by_text("Search for Class Sections", 1):
                            pass
                    if b.is_element_not_present_by_id("VAR1", 5):
                        b.execute_script("window.location.reload()")
                        sleep(2)
                        continue
                    selectDropdown("VAR1", semester)
                    selectDropdown("LIST_VAR1_1", subject)
                    selectDropdown("VAR6", "DSU")
                    b.find_by_id("WASubmit").first.click()
                    badResult = False
                    while not badResult and b.is_element_not_present_by_text("Section Selection Results", 1):
                        badResult = b.is_text_present("No classes meeting the search criteria have been found.")
                    if badResult:
                        unsuccessfulRun = False
                        continue
                    currentPage, totalPages = "0", "1"
                    courses = []
                    while currentPage != totalPages:
                        while b.is_element_not_present_by_css('table[summary="Paged Table Navigation Area"]'):
                            pass
                        m = match(r"Page (\d+) of (\d+)", b.find_by_css('table[summary="Paged Table Navigation Area"]').first.find_by_css('td[align="right"]').first.text)
                        currentPage, totalPages = m.groups(1)
                        courses.extend(scrapeTable(b.find_by_css('table[summary="Sections"]')))
                        handleExtraExits()
                        b.find_by_css('input[value="NEXT"]').first.click()
                        if currentPage != totalPages:
                            while not b.is_text_present("Page {0} of {1}".format(int(currentPage) + 1, totalPages)):
                                sleep(1)
                    subjectCourses[subject] = {}
                    for c in courses:
                        if c.CourseCode in subjectCourses[subject]:
                            subjectCourses[subject][c.CourseCode].append({c.CourseCodeSection: objectToDict(c)})
                        else:
                            subjectCourses[subject][c.CourseCode] = [{c.CourseCodeSection: objectToDict(c)}]
                    unsuccessfulRun = False
                except Exception as e:
                    print("Trying again after error: \n{0}".format(e))
                    b.execute_script("window.location.reload()")
                    while b.is_element_not_present_by_text("Search for Class Sections", 1):
                        pass
                        #You are not logged in. You must be logged in to access information. Try refreshing the page in your browser.
        totalData[semester] = subjectCourses
    for t in teachers:
        totalData["Teachers"][teachers[t].Email] = objectToDict(teachers[t])
    with open("test2.json", "w") as fi:
        json.dump(totalData, fi)

def initToQuery():
    url = "https://portal.sdbor.edu"
    b.visit(url)
    b.find_by_id("username").fill(config["wa_username"])
    b.find_by_id("password").fill(config["wa_password"] + "\n")
    b.visit(url + "/dsu-student/Pages/default.aspx")
    while b.is_element_not_present_by_text("WebAdvisor for Prospective Students"):
        pass
    b.find_by_text("WebAdvisor for Prospective Students").click()
    while b.is_element_not_present_by_text("Admission Information", 1):
        pass
    b.find_by_text("Admission Information").first.click()
    while b.is_element_not_present_by_text("Search for Class Sections", 1):
        pass
    b.find_by_text("Search for Class Sections").first.click()
    while b.is_element_not_present_by_id("VAR1", 1):
        pass

def getSemesters(): #assumes that you're already on the Prospective students search page
    select = b.find_by_id("VAR1")
    options = select.first.find_by_tag("option")
    return [x['value'] for x in options if x.text]

def getSubjects(): #assumes that you're already on the Prospective students search page
    select = b.find_by_id("LIST_VAR1_1")
    options = select.first.find_by_tag("option")
    return [x['value'] for x in options if x.text]

def handleExtraExits():
    for e, x in enumerate(b.find_by_css('button[type="button"][class="btn btn-sm btn-danger"]')):
        if e:
            x.click()
    
def objectToDict(obj):
    return {attr: getattr(obj, attr) for attr in type(obj).__dict__ if not attr.startswith("__")}

def scrapeTable(tab):
    courses = []
    for n in range(len(tab.find_by_tag("tr"))):
        if n == 0:
            continue
        course = Course()
        for e in range(len(tab.find_by_tag("tr")[n].find_by_tag("td"))):
            if e == 2:
                course.Open = "Open" in tab.find_by_tag("tr")[n].find_by_tag("td")[e].text
            elif e == 3:
                tab.find_by_tag("tr")[n].find_by_tag("td")[e].find_by_tag("a").first.click()
                while b.is_element_not_present_by_text("Section Information", 1):
                    print("Waiting on Section Information")
                #Start scraping for the bulk of the course data
                course.CourseName = b.find_by_id("VAR1").first.text
                course.CourseCodeSection = b.find_by_id("VAR2").first.text
                nm = match(r"(.+-.+)-.+", course.CourseCodeSection)
                course.CourseCode = nm.groups(1)[0] if nm is not None else course.CourseCodeSection
                course.CourseDescription = b.find_by_id("VAR3").first.text
                course.Location = b.find_by_id("VAR40").first.text
                course.Credits = float(b.find_by_id("VAR4").first.text)
                course.DateStart = b.find_by_id("VAR6").first.text
                course.DateEnd = b.find_by_id("VAR7").first.text
                course.MeetingInformation = b.find_by_id("LIST_VAR12_1").first.text
                course.TimeStart, course.TimeEnd = getTimes(course.MeetingInformation)
                course.Supplies = b.find_by_id("LIST_VAR14_1").first.text

                course.ReqConcurrentCourses = getConcurrent(b.find_by_id("VAR_LIST7_1").first.text)
                course.RecConcurrentCourses = getConcurrent(b.find_by_id("VAR_LIST8_1").first.text)
                course.PrereqCourses = getPrereqs(b.find_by_id("VAR_LIST1_1").first.text)
                PrereqNonCourse = b.find_by_id("VAR_LIST4_1").first.text
                course.PrereqNonCourse = PrereqNonCourse if PrereqNonCourse.lower() != "none" else ""
                teacherDicts = {}
                teacherDicts = scrapeTeachers(b.find_by_css('table[summary="Faculty Contact"]').first)
                for t in teacherDicts:
                    email = t["email"]
                    if not email: 
                        continue
                    name = t["name"]
                    phone = t["phone"]
                    course.InstructionalMethods[email] = t["lecture"]
                    if email:
                        course.ProfessorEmails.append(email)
                        if email not in teachers:
                            teachers[email] = Teacher(email, name, phone)
                        else:
                            if not teachers[email].Email:
                                teachers[email].Email = email
                            if not teachers[email].Name and name:
                                teachers[email].Name = name
                            if not teachers[email].Phone and phone:
                                teachers[email].Phone = phone
                handleExtraExits()
            elif e == 7:
                nm = match(r"(-?\d+) +\/ +(-?\d+) +\/ +(-?\d+)", tab.find_by_tag("tr")[n].find_by_tag("td")[e].text)
                if nm is None:
                    course.SlotsAvailable = course.SlotsCapacity = course.SlotsWaitlist = ""
                else:
                    course.SlotsAvailable = int(nm.group(1))
                    course.SlotsCapacity = int(nm.group(2))
                    course.SlotsWaitlist = int(nm.group(3))
            elif e == 9:
                course.AcademicLevel = tab.find_by_tag("tr")[n].find_by_tag("td")[e].text
        courses.append(course)
    sys.stdout.flush()
    return courses

def scrapeTeachers(tab):
    if tab is None:
        return []
    rl = []
    for e, tr in enumerate(tab.find_by_tag("tr")):
        if e:
            tdict = {}
            for n, td in enumerate(tr.find_by_tag("td")):
                if n == 1:
                    tdict["name"] = td.find_by_tag("div").first.text
                if n == 2:
                    tdict["phone"] = td.find_by_tag("div").first.text
                if n == 4:
                    tdict["email"] = td.find_by_tag("div").first.text
                if n == 5:
                    tdict["lecture"] = td.find_by_tag("div").first.text
            rl.append(tdict)
    return rl

def getPrereqs(s):
    AND = re.findall(r"\+ (\w+ \d+) ?", s) + re.findall(r"^(\w+ \d+) \+", s) + re.findall(r"^(\w+ \d+)$", s)
    OR  = re.findall(r"\+ (\w+ \d+) ?", s) + re.findall(r"^(\w+ \d+) \+", s)
    return {"and": replaceSpaces(AND), "or": replaceSpaces(OR)}

def replaceSpaces(l):
    return list(map(lambda a: a.replace(" ", "-"), l))

def getConcurrent(s):
    return replaceSpaces(re.findall(r"(\w+ \d+)", s))

def getTimes(s):
	r = re.search(r"(\d\d\:\d\d)(\w\w) - (\d\d\:\d\d)(\w\w)", s)
	if r is None:
		return (0, 0)
	st, et = int(r.group(1).replace(":", "")), int(r.group(3).replace(":", ""))
	if r.group(2).lower() == "pm":
		st += 1200
	if r.group(4).lower() == "pm":
		et += 1200
	return (st, et)


def selectDropdown(ddt, t): #assumes that you're already on the Prospective students search page
    selects = b.find_by_id(ddt).first
    selects.select(t)

if __name__ == "__main__":
    main()