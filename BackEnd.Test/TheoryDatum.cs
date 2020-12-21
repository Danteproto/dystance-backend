using System;
using System.Collections.Generic;
using System.Text;

namespace BackEnd.Test
{
    public interface ITheoryDatum
    {
        object[] ToParameterArray();
    }

    public abstract class TheoryDatum : ITheoryDatum
    {
        public abstract object[] ToParameterArray();


        public static ITheoryDatum Factory<TSystemUnderTest>(TSystemUnderTest sut, string description)
        {
            var datum = new TheoryDatum<TSystemUnderTest>();
            datum.SystemUnderTest = sut;
            datum.Description = description;
            return datum;
        }
    }

    public class TheoryDatum<TSystemUnderTest> : TheoryDatum
    {
        public TSystemUnderTest SystemUnderTest { get; set; }

        public string Description { get; set; }

        public override object[] ToParameterArray()
        {
            var output = new object[2];
            output[0] = SystemUnderTest;
            output[1] = Description;
            return output;
        }

    }
}
