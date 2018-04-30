﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DSUScheduleBuilder.Drawing
{
    using Models;
    using Utils;

    class WeekCourseView
    {

        /// <summary>
        /// The WeekView object that this CourseView belongs to
        /// </summary>
        private WeekView weekView;

        /// <summary>
        /// Size and positioning variables for the course
        /// </summary>
        private int x, y, w, h;
        /// <summary>
        /// List of what days the course is offered
        /// </summary>
        private List<int> days;

        /// <summary>
        /// The text displayed
        /// </summary>
        private string text;
        /// <summary>
        /// The color drawn behind the text
        /// </summary>
        private SolidBrush color;
        
        public WeekCourseView(WeekView wv, Random r = null)
        {
            this.weekView = wv;
            days = new List<int>();

            if (r == null)
            {
                r = new Random();
            }

            color = new SolidBrush(Color.FromArgb(255, 150 + (int)(r.NextDouble() * 100)
                                                     , 150 + (int)(r.NextDouble() * 100)
                                                     , 150 + (int)(r.NextDouble() * 100)));
        }

        private Course _course;
        /// <summary>
        /// The underlying course being drawn
        /// </summary>
        public Course Course
        {
            get { return _course; }
            set
            {
                _course = value;

                days.Clear();
                if (_course.DaysOfWeek.Contains("|mon|")) days.Add(0);
                if (_course.DaysOfWeek.Contains("|tues|")) days.Add(1);
                if (_course.DaysOfWeek.Contains("|wed|")) days.Add(2);
                if (_course.DaysOfWeek.Contains("|thur|")) days.Add(3);
                if (_course.DaysOfWeek.Contains("|fri|")) days.Add(4);

                int baseMin = Converter.TimeIntToMinutes(800);
                int startMin = Converter.TimeIntToMinutes(_course.StartTime) - baseMin;
                int endMin = Converter.TimeIntToMinutes(_course.EndTime) - baseMin;
                int duration = endMin - startMin;

                int baseX = weekView.TimeSlotWidth;
                int baseY = weekView.DayOfWeekHeight;

                x = baseX;
                y = (int)((startMin / 60.0) * weekView.TimeSlotHeight + baseY);
                w = weekView.CellWidth;
                h = (int)(weekView.TimeSlotHeight * (duration / 60.0));

                text = _course.CourseID + " : " + Converter.TimeIntToString(_course.StartTime) + " - " + Converter.TimeIntToString(_course.EndTime);
            }
        }
        
        /// <summary>
        /// Draws the course to the given graphics object
        /// </summary>
        /// <param name="graphics"></param>
        public void Draw(Graphics graphics)
        {
            Font drawFont = SystemFonts.DefaultFont;
            SizeF stringSize = graphics.MeasureString(text, drawFont);

            float textY = (h - stringSize.Height) / 2.0f + y;
            foreach (int d in days)
            {
                graphics.FillRectangle(color, x + weekView.CellWidth * d, y, w, h);
                float textX = weekView.CellWidth * d + (w - stringSize.Width) / 2.0f + x;
                graphics.DrawString(text, drawFont, Brushes.Black, textX, textY);
            }
        }
    }
}
