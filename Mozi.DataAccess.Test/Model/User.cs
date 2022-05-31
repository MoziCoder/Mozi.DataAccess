using System;

namespace Mozi.DataAccess.Test.Model
{
	public class User
    {
        public string UserId    { get; set; }
        public string NickName  { get; set; }
        public string Password  { get; set; }
        public string Mobile    { get; set; }
        public DateTime RegDate { get; set; }
        public int IsForbidden  { get; set; }
    }
}