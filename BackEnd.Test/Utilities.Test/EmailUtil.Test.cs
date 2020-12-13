using BackEnd.Ultilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BackEnd.Test.Utilities.Test
{
    public class EmailUtilTest
    {
        public EmailUtilTest()
        {
        }

        [Theory]
        [InlineData("testEmail@gmail.com")]
        [InlineData("testEmailWithNumbers0123456789@gmail.com")]
        [InlineData("testEmailWithSpecialCharacters!#$%^&*()@gmail.com")]
        [InlineData("testEmailWithNumbersAndSpecialCharacters0123456789!#$%^&*()@gmail.com")]
        [InlineData("testEmail@gmailWithNumbers0123456789.com")]
        [InlineData("testEmail@gmailWithSpecialCharacters.com")]
        [InlineData("testEmail@WithNumbersAndSpecialCharacters0123456789!.com")]
        public void CheckIfValid_Returns_Correctly(string email)
        {
            //Arrange


            //Act
            var result = EmailUtil.CheckIfValid(email);

            //Assert
            Assert.True(result);

        }


        [Theory]
        [InlineData("testEma@il@gmail.com")]
        [InlineData("testEmailWithNumbers01234@56789@gmail.com")]
        [InlineData("testEmailWithSpecialCharac@ters!#$%^&*()@gmail.com")]
        [InlineData("testEmailWithNumbersAndSpec@ialCharacters0123456789!#$%^&*()@gmail.com")]
        [InlineData("testEmail@gmailWithNumbers01@23456789.com")]
        [InlineData("testEmail@gmailWithSpecialCha@racters!#$%^&*().com")]
        [InlineData("testEmail@WithNumbersAndSpecia@lCharacters0123456789!.com")]
        public void CheckIfValid_Returns_Wrong(string email)
        {
            //Arrange


            //Act
            var result = EmailUtil.CheckIfValid(email);

            //Assert
            Assert.False(result);

        }
    }
}
