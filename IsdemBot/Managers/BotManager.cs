using IsdemBot.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace IsdemBot.Managers
{
    public class BotManager
    {
        private readonly IWebDriver _driver;

        public BotManager(IWebDriver driver)
        {
            _driver = driver;
        }

        private bool SendDataIsComplete
        {
            get
            {
                try
                {
                    var select = new SelectElement(_driver.FindElement(By.Id("faces:denunciationTypes_input")));

                    return select.SelectedOption.Text != "KİŞİ";
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool TcNoIsNotValid
        {
            get
            {
                try
                {
                    var errorMsg = _driver.FindElement(By.Id("faces:unitIdentifierMessage")).Text;

                    return !string.IsNullOrWhiteSpace(errorMsg);
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool DateTimeNotValid
        {
            get
            {
                try
                {
                    var errorMsg = _driver.FindElement(By.Id("faces:unitIdentifierMessage")).Text;

                    return !string.IsNullOrWhiteSpace(errorMsg) && errorMsg.Contains("Denetim zamanını girilmesi zorunludur");
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool IsGetUserData
        {
            get
            {
                try
                {
                    var userNameElement = _driver.FindElement(By.Id("faces:name"));

                    if (userNameElement == null)
                        return false;

                    return userNameElement.GetAttribute("value") != "";
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool PageIsReady
        {
            get
            {
                try
                {
                    _driver.FindElement(By.Id("faces:getfromkps"));

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool Connect()
        {
            try
            {
                _driver.Navigate().GoToUrl("https://isdem.ng112.gov.tr/NG112-Isdem/acil-destek/giris.xhtml");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Login(LoginUser user)
        {
            var userName = _driver.FindElement(By.Id("form:username"));
            var password = _driver.FindElement(By.Id("form:password"));

            userName.SendKeys(user.UserName);
            password.SendKeys(user.Password);

            _driver.FindElement(By.Id("form:authendicate")).Click();

            Thread.Sleep(1000);

            _driver.FindElement(By.Id("faces:audit")).Click();

            Thread.Sleep(2000);
        }

        public void SendData(string tcNo,string adres, DateTime date)
        {
            _driver.FindElement(By.Id("faces:denunciationTypes")).Click();

            var selectType = _driver.FindElement(By.Id("faces:denunciationTypes_filter"));

            selectType.SendKeys("KİŞİ");
            selectType.SendKeys(Keys.Down);
            selectType.SendKeys(Keys.Enter);

            while (!PageIsReady)
                Thread.Sleep(1000);

            _driver.FindElement(By.Id("faces:initIdentifier")).SendKeys(tcNo);
            Thread.Sleep(500);

            _driver.FindElement(By.Id("faces:getfromkps")).Click();

            while (!IsGetUserData)
            {
                if (TcNoIsNotValid)
                {
                    PageForNewData(date, true);
                    return;
                }

                Thread.Sleep(1000);
            }

            _driver.FindElement(By.Id("faces:denunciationExplanation")).SendKeys("UYGUN");

            //Ekip Seçimi
            _driver.FindElement(By.Id("faces:teams")).Click();
            Thread.Sleep(500);
            _driver.FindElement(By.Id("faces:teams_1")).Click();

            //Denetim Sonucu Seçimi
            _driver.FindElement(By.Id("faces:resultType")).Click();
            Thread.Sleep(500);
            _driver.FindElement(By.Id("faces:resultType_1")).Click();

            //İlçe Seçimi
            _driver.FindElement(By.Id("faces:county")).Click();
            Thread.Sleep(500);
            _driver.FindElement(By.Id("faces:county_5")).Click();

            Thread.Sleep(500);

            //Açık Adres
            var addressDetail = _driver.FindElement(By.Id("faces:adresDetail"));
            new Actions(_driver)
                .Click(addressDetail)
                .KeyDown(Keys.Control)
                .SendKeys("a")
                .KeyUp(Keys.Control)
                .SendKeys(adres)
                .Perform();

            Thread.Sleep(500);

            //Denetim Tarihi
            _driver.FindElement(By.Id("faces:denunciationTime_input")).SendKeys(RandomTime.Create(date));

            Thread.Sleep(500);

#if DEBUG
            PageForNewData(date, true);
#else
            PageForNewData(date);
#endif
        }

        public void Quit()
        {
            _driver.Quit();
        }

        public void PageReload()
        {
            _driver.FindElement(By.Id("faces:yenile")).Click();

            while (!SendDataIsComplete)
                Thread.Sleep(1000);
        }

        public void PageForNewData(DateTime date, bool isPageRefresh = false)
        {
            error:
            if (isPageRefresh)
                _driver.FindElement(By.Id("faces:yenile")).Click();
            else
                _driver.FindElement(By.Id("faces:save")).Click();

            while (!SendDataIsComplete)
            {
                Thread.Sleep(1000);

                if (DateTimeNotValid)
                {
                    //Denetim Tarihi
                    _driver.FindElement(By.Id("faces:denunciationTime_input")).SendKeys(RandomTime.Create(date));
                    Thread.Sleep(500);
                    goto error;
                }
            }
        }
    }
}
