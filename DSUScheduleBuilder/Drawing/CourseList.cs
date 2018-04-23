﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;


namespace DSUScheduleBuilder.Drawing
{
    using Models;
    using Utils;

    public enum CourseListState
    {
        ClassList,
        SpecificClass
    }

    public abstract class CourseList<T> : Control where T : Course
    {
        protected List<T> courses;
        protected int totalPages;
        protected int currPage;

        protected int cellWidth;
        protected int cellHeight = 1;
        protected int bottomBarHeight;

        private Rectangle backButtonRect;
        private Rectangle forwardButtonRect;

        protected CourseListState state;
        protected T selectedCourse;

        public CourseList()
        {

        }

        public virtual void SetCourses(List<T> cs)
        {
            this.courses = cs;
            this.totalPages = (this.courses.Count - 1) / 5;
            this.currPage = 0;

            this.bottomBarHeight = 32;
            this.cellWidth = this.Size.Width;
            this.cellHeight = (this.Size.Height - bottomBarHeight) / 5;
            if (cellHeight == 0) cellHeight = 1;

            int bx = (this.Size.Width / 2 - 32) / 2;
            backButtonRect = new Rectangle(bx, this.Size.Height - this.bottomBarHeight, 32, 32);
            bx += this.Size.Width / 2;
            forwardButtonRect = new Rectangle(bx, this.Size.Height - this.bottomBarHeight, 32, 32);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            switch (this.state)
            {
                case CourseListState.ClassList:
                    drawClassList(e.Graphics);
                    break;
                case CourseListState.SpecificClass:
                    drawSpecificClass(e.Graphics);
                    break;
                default:
                    break;
            }
        }

        private void drawSpecificClass(Graphics g)
        {
            if (selectedCourse == null)
            {
                state = CourseListState.ClassList;
                return;
            }

            g.FillRectangle(Brushes.Aqua, 0, 0, this.Size.Width, this.Size.Height);

            Font font = new Font(FontFamily.GenericSansSerif, 14);
            string text = selectedCourse.CourseID;
            SizeF textSize = g.MeasureString(text, font);

            g.DrawString(selectedCourse.CourseID, font, Brushes.Black, 4, 4 + (textSize.Height + 4) * 0);
            g.DrawString(selectedCourse.CourseName, font, Brushes.Black, 4, 4 + (textSize.Height + 4) * 1);
            if (selectedCourse.Teacher != null)
                g.DrawString(selectedCourse.Teacher, font, Brushes.Black, 4, 4 + (textSize.Height + 4) * 2);

            text = selectedCourse.DaysOfWeekPresent;
            textSize = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.Black, cellWidth - textSize.Width - 4, 4 + (textSize.Height + 4) * 2);

            if (text != "Online")
            {
                text = Converter.TimeIntToString(selectedCourse.StartTime) + " - " + Converter.TimeIntToString(selectedCourse.EndTime);
                textSize = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.Black, cellWidth - textSize.Width - 4, 4 + (textSize.Height + 4) * 0);
            }

            drawSelectedCourseExtra(g);
        }

        protected abstract void drawSelectedCourseExtra(Graphics g);

        protected void drawClassList(Graphics g)
        {
            if (courses != null)
            {
                int t = courses.Count - currPage * 5;
                int len = 5 < courses.Count ? (t < 5 ? t : 5) : courses.Count;
                for (int i = 0; i < len; i++)
                {
                    drawCourse(g, i, courses[i + currPage * 5]);
                }
            }

            if (totalPages > 0)
            {
                drawBottomBar(g);
            }
        }

        private void drawCourse(Graphics g, int number, T course)
        {
            g.FillRectangle(number % 2 == 0 ? Brushes.Aqua : Brushes.Aquamarine, 0, number * cellHeight, Size.Width, cellHeight);

            Font font = new Font(FontFamily.GenericSansSerif, 14);
            SizeF textSize;

            textSize = g.MeasureString(course.CourseID, font);

            g.DrawString(course.CourseID, font, Brushes.Black, 4, number * cellHeight + 4 + (textSize.Height + 4) * 0);
            g.DrawString(course.CourseName, font, Brushes.Black, 4, number * cellHeight + 4 + (textSize.Height + 4) * 1);
            if (course.Teacher != null)
                g.DrawString(course.Teacher, font, Brushes.Black, 4, number * cellHeight + 4 + (textSize.Height + 4) * 2);

            string text = course.DaysOfWeekPresent;
            textSize = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.Black, cellWidth - textSize.Width - 4, number * cellHeight + 4 + (textSize.Height + 4) * 2);

            //Don't draw times for online courses
            if (text != "Online")
            {
                text = Converter.TimeIntToString(course.StartTime) + " - " + Converter.TimeIntToString(course.EndTime);
                textSize = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.Black, cellWidth - textSize.Width - 4, number * cellHeight + 4 + (textSize.Height + 4) * 0);
            }
        }

        private void drawBottomBar(Graphics g)
        {
            int topY = this.Size.Height - bottomBarHeight;

            Font font = new Font(FontFamily.GenericSansSerif, 12);
            string text = (currPage + 1) + " of " + (totalPages + 1) + " pages";
            SizeF textSize = g.MeasureString(text, font);

            g.DrawString(text, font, Brushes.White, (this.Size.Width - textSize.Width) / 2.0f, topY + 2);

            font = new Font(FontFamily.GenericMonospace, 24);
            g.FillRectangle(Brushes.Aquamarine, backButtonRect);
            g.DrawString("<", font, Brushes.Black, backButtonRect.X, backButtonRect.Y);
            g.FillRectangle(Brushes.Aquamarine, forwardButtonRect);
            g.DrawString(">", font, Brushes.Black, forwardButtonRect.X, forwardButtonRect.Y);
        }

        protected bool CheckBottomBarClick(int mx, int my)
        {
            if (courses == null) return true;
            if (backButtonRect.Contains(mx, my))
            {
                this.currPage -= 1;
                if (currPage < 0) currPage = 0;
                return true;
            }

            if (forwardButtonRect.Contains(mx, my))
            {
                this.currPage += 1;
                if (currPage > totalPages) currPage = totalPages;
                return true;
            }
            return false;
        }

        protected abstract void HandleClick(int mx, int my);
        public void OnClick(EventArgs e)
        {
            int sx = PointToScreen(Point.Empty).X;
            int sy = PointToScreen(Point.Empty).Y;

            int mx = MousePosition.X - sx;
            int my = MousePosition.Y - sy;
            HandleClick(mx, my);
        }
    }

}
