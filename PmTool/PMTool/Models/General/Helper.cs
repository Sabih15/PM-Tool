using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PMTool.General
{
    public static class Helper
    {
        #region Helper Functions

        public static string EncryptPrimaryKey(int value)
        {
            return EncryptDecrypt.Encrypt(value.ToString());
        }
        //public static string GetIPAddress()
        //{
        //    return HttpContext.Current.Request.UserHostAddress;
        //}

        public static string Serialize(object obj)
        {
            string result = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return result;
        }

        public static object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static decimal? GetOffsetValue(string offsetValue)
        {
            int serverOffset = Convert.ToInt32(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes);
            int clientOffset = string.IsNullOrEmpty(offsetValue) ? 0 : int.Parse(offsetValue);

            return Math.Abs(serverOffset - clientOffset) / 60;
        }

        public static string DecrString(string V)
        {
            return EncryptDecrypt.Decrypt(V);
        }

        public static List<int> GetListOfInterger(string text)
        {
            List<int> mylist = new List<int>();
            if (text == null)
            {
                return mylist;
            }

            string[] Ids = text.Split(',');
            if ((text != "") && (text != null))
            {
                foreach (string word in Ids)
                {
                    mylist.Add(Convert.ToInt32(word));

                }
            }
            return mylist;

        }

        public static DateTime? ConvertDate(object Date)
        {
            string[] d = Date.ToString().Split(' ');
            if (d.Length > 1)
            {
                string[] f = d[0].Split('/');
                Date = f[1] + "/" + f[0] + "/" + f[2] + " " + d[1];
                return Convert.ToDateTime(Date);
            }
            else
            {
                string[] f = Date.ToString().Split('/');
                Date = f[1] + "/" + f[0] + "/" + f[2];
                return Convert.ToDateTime(Date);
            }
        }

        public static DateTime ConvertDateTime(object Datetime)
        {
            string[] d = Datetime.ToString().Split(' ');
            if (d.Length > 1)
            {
                DateTime dt;
                bool res = DateTime.TryParse(d[4] + " " + d[5], out dt);
                Datetime = d[0] + "-" + d[1] + "-" + d[2] + " " + dt.ToString("HH:mm");
            }
            return Convert.ToDateTime(Datetime);
        }

        //public static string GetHash(string value)
        //{
        //    return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(value, "md5");
        //}

        public static string GetEnumDescription(Enum value)
        {
            if (Convert.ToInt32(value) > 0)
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

                if (attributes != null &&
                    attributes.Length > 0)
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            return "";
        }

        public static string FormatCurrency2(decimal? Amount)
        {
            if (!Amount.HasValue)
                return "0.00";
            else
                return Amount.Value.ToString("#,##0.00");
        }

        public static string GetResetKey(int? length = null)
        {
            Random rnd = new Random();
            string ResetKey = "";
            for (int i = 0; i < (length != null ? length : 4); i++)
            {
                ResetKey += rnd.Next(0, 9);
            }
            return ResetKey;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        //public static Object JSDeserialize(Type type, string obj)
        //{
        //    JsonConvert.DeserializeObject()
        //   // System.Web.Script.Serialization.JavaScriptSerializer parser = new System.Web.Script.Serialization.JavaScriptSerializer();
        //   // return parser.Deserialize(obj, type);
        //}

    public static double getDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = deg2rad(lon2 - lon1);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }

        public static string GetRandomAplhanumericString(int size)
        {
            char[] chars =
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }

            return result.ToString();
        }

        public static string GetRandomNumericString(int size)
        {
            char[] chars =
                    "1234567890".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }

            return result.ToString();
        }

        //public static int GetUserIdFromToken(System.Net.Http.HttpRequestMessage Request)
        //{
        //    return JwtManager.GetUserIdFromToken(Request.Headers.Authorization.Parameter);
        //}

        public static List<DateTime> GetAllDatesInMonth(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day).Date) // Map each day to a date
                             .ToList(); // Load dates into a list
        }

        public static double GetTotalHours(DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                var timeSpan = (ToDate - FromDate);
                var TotalWorkingHours = timeSpan.HasValue ? timeSpan.Value.TotalHours : 0;
                return TotalWorkingHours;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static List<T> ShuffleList<T>(List<T> inputList)
        {
            List<T> randomList = new List<T>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }

        #endregion Help Functions

        #region All Enums
        public enum ResponseEnum
        {
            InvalidInformation = 1,
            UnAuthorized = 2,
            DefaultErrorMsg = 3,
            GetSuccess = 4,
            CreateSuccess = 5,
            UpdateSuccess = 6,
            DeleteSuccess = 7,
            TokenExpired = 8,
            UserExist = 9,
            NotASocialUser = 10,
            NotFound = 11,
            EmailNotSent = 12,
            ProjectExist = 13,
            TeamExist = 14,
            IsASocialUser = 15,
            InvalidLogin = 16,
            SignUpSuccess = 17,
            TeamInvitationExpired = 18,
            ProjectInvitationExpired = 19
        }
        public enum EmailTemplates
        {
            ForgotPassword = 1,
            VerifyEmail = 2,
            InviteEmail = 3,
            ProjectDueToday = 4,
            ProjectDueTomorow = 5,
            CardDueToday = 6,
            CardDueTomorrow = 7
        }

        public enum Gender
        {
            Male = 1,
            M = 1,
            Female = 2,
            F = 2
        }

        #endregion
    }
}
