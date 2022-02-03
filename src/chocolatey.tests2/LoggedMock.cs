using chocolatey.tests2;

namespace Moq
{
    public static class MockExtensions
    {
        static Castle.DynamicProxy.ProxyGenerator proxygenerator;
        
        /// <summary>
        /// Enables logging of specific mock instance
        /// </summary>
        /// <param name="methodsToIgnore">Specifies method or property names to ignore when logging 
        /// (e.g. utility functions about which we don't care)</param>
        public static Mock<T> Logged<T>(this Mock<T> mock, params string[] methodsToIgnore) where T : class
        {
            object mockedObject = mock.Object;
            object proxy;

            if (proxygenerator == null)
            {
                proxygenerator = new Castle.DynamicProxy.ProxyGenerator();
            }

            if (typeof(T).IsInterface)
            {
                proxy = proxygenerator.CreateInterfaceProxyWithTarget(typeof(T), mockedObject, new LoggedMockInterceptor(methodsToIgnore));
            }
            else
            {
                proxy = proxygenerator.CreateClassProxyWithTarget(typeof(T), mockedObject, new LoggedMockInterceptor(methodsToIgnore));
            }

            var field = mock.GetType().GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(mock, proxy);
            return mock;
        }
    }
}

