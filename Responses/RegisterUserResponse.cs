using BackEnd.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    public class RegisterUserResponse : UseCaseResponseMessage
    {
        public string Id { get; }
        public IEnumerable<string> Errors { get; }

        public RegisterUserResponse(IEnumerable<string> errors, bool success = false, string message = null) : base(success, message)
        {
            Errors = errors;
        }

        public RegisterUserResponse(string id, bool success = false, string message = null) : base(success, message)
        {
            Id = id;
        }
    }
}
