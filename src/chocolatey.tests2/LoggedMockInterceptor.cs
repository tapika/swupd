using Castle.DynamicProxy;
using chocolatey.infrastructure.logging;
using System.Linq;
using System.Reflection;
using System.Text;

namespace chocolatey.tests2
{
    public class LoggedMockInterceptor : IInterceptor
    {
        StringBuilder sb = new StringBuilder();
        string[] _methodsToIgnore;

        public LoggedMockInterceptor(string[] methodsToIgnore)
        {
            _methodsToIgnore = methodsToIgnore;
        }

        public void Intercept(IInvocation invocation)
        {
            string name = invocation.Method.Name;

            if (_methodsToIgnore.Contains(name))
            {
                invocation.Proceed();
                return;
            }

            ParameterInfo[] parameters = invocation.Method.GetParameters();
            sb.Clear();

            object[] args = invocation.Arguments;
            sb.Append(name);
            sb.Append("(");

            for (int i = 0; i < parameters.Length; i++)
            {
                if (i != 0)
                {
                    sb.Append(",");
                }

                if (parameters[i].ParameterType == typeof(string))
                {
                    if (args[i] == null)
                    {
                        sb.Append("null");
                    }
                    else
                    { 
                        sb.Append("\"");
                        string s = args[i].ToString();
                        s = s.Replace(System.IO.Path.DirectorySeparatorChar, '/');
                        sb.Append(s);
                        sb.Append("\"");
                    }
                }
                else
                { 
                    sb.Append(args[i].ToString());
                }
            }
            
            sb.Append(")");
        
            LogService.GetInstance(false)._console.Info(sb.ToString());
            invocation.Proceed();
        }
    }
}
