using BackEnd.Ultilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BackEnd.Test.Utilities.Test
{
    public class DateTimeUtilTest
    {
        public DateTimeUtilTest()
        {

        }


        [Theory]
        [InlineData("2019-02-02",
            "2019-02-10",
            "2019-02-02,2019-02-03,2019-02-04,2019-02-05,2019-02-06,2019-02-07,2019-02-08,2019-02-09,2019-02-10")]
        [InlineData("1970-02-02",
            "1970-02-10",
            "1970-02-02,1970-02-03,1970-02-04,1970-02-05,1970-02-06,1970-02-07,1970-02-08,1970-02-09,1970-02-10")]
        public void EachDay_Returns_Correctly(string from, string to, string expected)
        {
            //Arrange
            var fromDate = DateTime.Parse(from);
            var toDate = DateTime.Parse(to);
            List<DateTime> dateTimes = new List<DateTime>();
            foreach(var d in expected.Split(','))
            {
                dateTimes.Add(DateTime.Parse(d));
            }

            //Act
            var result = DateTimeUtil.EachDay(fromDate, toDate);

            //Assert
            Assert.Equal(result, dateTimes.ToArray());
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("2019-02-02",
            "2019-02-10",
            "2019-02-02,2019-02-03,2019-02-04,2019-02-05,2019-02-06,2019-02-07,2019-02-08,2019-02-09")]
        [InlineData("1970-02-02",
            "1970-02-10",
            "1970-02-02,1970-02-03,1970-02-04,1970-02-05,1970-02-06,1970-02-07,1970-02-08,1970-02-09")]
        public void EachDay_Returns_Wrong(string from, string to, string expected)
        {
            //Arrange
            var fromDate = DateTime.Parse(from);
            var toDate = DateTime.Parse(to);
            List<DateTime> dateTimes = new List<DateTime>();
            foreach (var d in expected.Split(','))
            {
                dateTimes.Add(DateTime.Parse(d));
            }

            //Act
            var result = DateTimeUtil.EachDay(fromDate, toDate);

            //Assert
            Assert.NotEqual(result, dateTimes.ToArray());
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("",
            "",
            "1/1/0001 00:00:00")]
        public void EachDay_Returns_0001Date_WhenEmpty(string from, string to, string expected)
        {
            //Arrange
            DateTime fromDate;
            DateTime toDate;
            DateTime.TryParse(from, out fromDate);
            DateTime.TryParse(to, out toDate);

            //Act
            var result = DateTimeUtil.EachDay(fromDate, toDate);
            //Assert
            Assert.Equal(result.ToList()[0].ToString(), expected);
        }


        [Theory]
        [InlineData("31.05.2016 13:33:00", "2020-02-02T00:00:00")]
        public void GetDateTimeFromString_Returns_Correctly(string date, string expected)
        {
            //Arrange


            //Act

            //Assert
            Assert.Throws<FormatException>(() => DateTimeUtil.GetDateTimeFromString(date));
        }
    }
}
