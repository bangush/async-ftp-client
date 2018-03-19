using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public class ReflectionUtils
    {
        public static void GetMyProperties(object obj)
        {
            foreach (PropertyInfo pinfo in obj.GetType().GetProperties())
            {
                var getMethod = pinfo.GetGetMethod();
                if (getMethod.ReturnType.IsArray)
                {
                    var arrayObject = getMethod.Invoke(obj, null);
                    foreach (object element in (Array)arrayObject)
                    {
                        foreach (PropertyInfo arrayObjPinfo in element.GetType().GetProperties())
                        {
                            Console.WriteLine(arrayObjPinfo.Name + ":" + arrayObjPinfo.GetGetMethod().Invoke(element, null).ToString());
                        }
                    }
                }
            }
        }
    }
}
