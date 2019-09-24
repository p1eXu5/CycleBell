/*
 * Using Appium WebDriver
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;

namespace CycleBell.Tests.FunctionalTests
{
    [TestFixture]
    public class UiTests
    {
        protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string CycleBellAppId = @"D:\Projects\Programming Projects\C# Projects\WPF\CycleBell\Build\CycleBell.exe";

        protected static WindowsDriver<WindowsElement> session;
        protected static WindowsElement editBox;


        [SetUp]
        public static void SetUp ()
        {
            // Launch a new instance of Notepad application
            if (session == null)
            {
                // Create a new session to launch Notepad application
                DesiredCapabilities appCapabilities = new DesiredCapabilities();
                appCapabilities.SetCapability("app", CycleBellAppId);
                //session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities);
                Assert.IsNotNull(session);
                Assert.IsNotNull(session.SessionId);

                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times
                session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);

                // Keep track of the edit box to be used throughout the session
                editBox = session.FindElementByWindowsUIAutomation("timer_Box");
                Assert.IsNotNull(editBox);
            }
        }

        [TearDown]
        public static void TearDown()
        {
            // Close the application and delete the session
            if (session != null)
            {
                session.Close();

                try
                {
                    // Dismiss Save dialog if it is blocking the exit
                    //session.FindElementByName("Don't Save").Click();
                }
                catch { }

                session.Quit();
                session = null;
            }
        }

        [Test]
        public void TestInitialize()
        {
            // Select all text and delete to clear the edit box
            editBox.SendKeys(Keys.Control + "a" + Keys.Control);
            editBox.SendKeys(Keys.Delete);
            //Assert.AreEqual(string.Empty, editBox.Text);
        }
    }
}
