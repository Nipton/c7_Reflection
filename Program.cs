using System.Text;
using static c7_Reflection.Program;

namespace c7_Reflection
{
    class TestClass
    {
        [CustomName("Целое число")]
        public int I { get; set; }
        public string? S { get; set; }
        public decimal D { get; set; }
        [CustomName("Массив")]
        public char[]? C { get; set; }

        public TestClass()
        { }
        private TestClass(int i)
        {
            this.I = i;
        }
        public TestClass(int i, string s, decimal d, char[] c) : this(i)
        {
            this.S = s;
            this.D = d;
            this.C = c;
        }
    }
    internal class Program
    {
        public static TestClass? CreationInstance()
        {
            Type t = typeof(TestClass);
            return Activator.CreateInstance(t) as TestClass;
        }
        public static TestClass? CreationInstance(int i)
        {
            Type t = typeof(TestClass);
            return Activator.CreateInstance(t, new object[] { i }) as TestClass;
        }
        public static TestClass? CreationInstance(int i, string s, decimal d, char[] c)
        {
            Type t = typeof(TestClass);
            return Activator.CreateInstance(t, new object[] { i, s, d, c }) as TestClass;
        }
        static object? StringToObject(string s)
        {
            var arr = s.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var obj = Activator.CreateInstance(arr[1], arr[0])?.Unwrap();
            var type = obj?.GetType();
            if (arr.Length > 2 && type != null)
            {
                for (int i = 2; i < arr.Length; i++)
                {
                    string[] propertyAndValue = arr[i].Split(':');
                    var prop = type.GetProperty(propertyAndValue[0]);
                    if(prop == null)
                    {
                        var properties = type.GetProperties();
                        foreach (var property in properties)
                        {
                            var attributes = property.GetCustomAttributes(false);
                            foreach (var attribute in attributes)
                            {
                                if(attribute is CustomNameAttribute cna && cna.CustomName == propertyAndValue[0])
                                {
                                    if (property.PropertyType == typeof(int))
                                    {
                                        property.SetValue(obj, int.Parse(propertyAndValue[1]));
                                    }
                                    else if (property.PropertyType == typeof(decimal))
                                    {
                                        property.SetValue(obj, decimal.Parse(propertyAndValue[1]));
                                    }
                                    else if (property.PropertyType == typeof(char[]))
                                    {
                                        property.SetValue(obj, propertyAndValue[1].Replace(", ", "").ToCharArray());
                                    }
                                    else if (property.PropertyType == typeof(string))
                                    {
                                        property.SetValue(obj, propertyAndValue[1]);
                                    }
                                }
                            }
                        }
                    }
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(obj, int.Parse(propertyAndValue[1]));
                        }
                        else if (prop.PropertyType == typeof(decimal))
                        {
                            prop.SetValue(obj, decimal.Parse(propertyAndValue[1]));
                        }
                        else if (prop.PropertyType == typeof(char[]))
                        {
                            prop.SetValue(obj, propertyAndValue[1].Replace(", ", "").ToCharArray());
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(obj, propertyAndValue[1]);
                        }
                    }
                }
            }
            return obj;
        }
        //“TestClass|test2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null|I:1|S:STR|D:2.0|”
        static string ObjectToString(object o)
        {
            Type type = o.GetType();
            StringBuilder result = new StringBuilder();
            result.Append(type.FullName + "|");
            result.Append(type.Assembly);
            foreach (var prop in type.GetProperties())
            {
                result.Append("|");
                var attributes = prop.GetCustomAttributes(false);
                if (attributes.Length != 0)
                {
                    foreach (Attribute attribute in attributes)
                    {
                        if (attribute is CustomNameAttribute cna)
                        {
                            result.Append(cna.CustomName);
                        }
                    }
                }
                else
                    result.Append(prop.Name);
                result.Append(":");
                if (prop.PropertyType.IsArray)
                {
                    result.Append(string.Join(", ", (prop.GetValue(o) as char[]) ?? new char[] { }));
                }
                else
                    result.Append(prop.GetValue(o));
                result.Append("|");
            }
            return result.ToString();
        }
        [AttributeUsage(AttributeTargets.Property)]
        public class CustomNameAttribute : Attribute
        {
            public string CustomName { get; set; }
            public CustomNameAttribute(string customName) 
            {
                CustomName = customName;
            }
        }
        static void Main(string[] args)
        {
            TestClass tc = new TestClass(3, "sad", 7, new char[] { 'a', 'b' });
            Console.WriteLine(ObjectToString(tc));
            TestClass? t = StringToObject(ObjectToString(tc)) as TestClass;
            Console.Write($"{t.I} {t.S} {t.D} ");
            foreach (char c in t.C)
                Console.Write(c);

        }
    }
}