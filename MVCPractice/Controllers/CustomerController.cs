using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer;
using System.Net.Http;
using System.Net.Http.Headers;
using MVCPractice.Models;



namespace MVCPractice.Controllers
{
    public class CustomerController : Controller
    {
        HttpClient client = new HttpClient();
        
        public ActionResult Index()
        {
            List<accountDetailViewModel> obj = new List<accountDetailViewModel>();
            using (client)
            {
                client.BaseAddress = new Uri("http://localhost:80");
                //HTTP GET
                var responseTask = client.GetAsync("BankAPI/api/Customer/getAccountsOfUser?loginId="+Session["emailId"].ToString());
                responseTask.Wait();    
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<List<accountDetailViewModel>>();
                    readTask.Wait();
                    obj = readTask.Result;
                }
                else 
                {
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            Session["medal"] = null;
            return View(obj);
        }



        [HttpPost]
        public ActionResult Index(string selectedAccount)
        {
            if (string.IsNullOrEmpty(selectedAccount))
            {
                
                Session["NoAccountSelectedError"] = "Please select the account";
                return RedirectToAction("Index");
            }
            else
            {
                Session["accountNumber"] = selectedAccount;
                long selectedAccountLong = long.Parse(selectedAccount);
                accountDetailViewModel obj = new accountDetailViewModel();
                accountTypeDetailViewModel cmedal = new accountTypeDetailViewModel();
                using (client)
                {
                    client.BaseAddress = new Uri("http://localhost:80");
                    //HTTP GET
                    var responseTask = client.GetAsync("BankAPI/api/Customer/getSpecificAccountOfUser?accountNo=" + selectedAccountLong);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<accountDetailViewModel>();
                        readTask.Wait();
                        obj = readTask.Result;
                        responseTask = client.GetAsync("BankAPI/api/Customer/getAccountTypeOfGivenAmount?amount=" + long.Parse(obj.accountBalance.ToString()));
                        responseTask.Wait();
                        result = responseTask.Result;
                        var readTask1 = result.Content.ReadAsAsync<accountTypeDetailViewModel>();
                        readTask1.Wait();
                        cmedal = readTask1.Result;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                Session["medal"] = cmedal.accountType;
                return RedirectToAction("Menu");
            }



        }
        public ActionResult Menu()
        {
            if (Session["accountNumber"] != null)
            {
                long accountNo = long.Parse((Session["accountNumber"]).ToString());
                accountDetailViewModel obj = new accountDetailViewModel();
                using (client)
                {
                    client.BaseAddress = new Uri("http://localhost:80");
                    //HTTP GET
                    var responseTask = client.GetAsync("BankAPI/api/Customer/getSpecificAccountOfUser?accountNo=" + accountNo);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<accountDetailViewModel>();
                        readTask.Wait();
                        obj = readTask.Result;

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                return View(obj);
            }
            else
            {
                return View("Index");
            }
        }

        public ActionResult FundTransfer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FundTransfer(long destinationAccountNo, int amount, string comment)
        {
            bool obj,res=false;
            int obj1=0;
            long accountNo = long.Parse(Session["accountNumber"].ToString());
            try
            {
                if (accountNo == destinationAccountNo)
                {
                    Session["SameAccountFundTransferError"] = "Source and destination account cant be same";
                    return View();
                }
               // bool res = obj.checkAccount(destinationAccountNo);
                using (client)
                {
                    client.BaseAddress = new Uri("http://localhost:80");
                    //HTTP GET
                    var responseTask = client.GetAsync("BankAPI/api/Customer/checkDestinationAccount?accountNo=" + destinationAccountNo);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<bool>();
                        readTask.Wait();
                        obj = readTask.Result;
                        res = obj;
                        responseTask = client.GetAsync("BankAPI/api/Customer/getAmount?accountNo=" + accountNo);
                        responseTask.Wait();
                        result = responseTask.Result;
                        var readTask1 = result.Content.ReadAsAsync<int>();
                        readTask1.Wait();
                        obj1 = readTask1.Result;

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                
                if (res)
                {
                    int amt = obj1;
                    if (amt > amount)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("http://localhost:80");
                            //HTTP GET
                            var responseTask = client.PostAsJsonAsync("BankAPI/api/Customer/FundTransfer?amount=" + amount + "&destinationAccountNo=" + destinationAccountNo + "&SourceAccountNo=" + accountNo,amount);
                            responseTask.Wait();
                            var result = responseTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                Session["FundTransferSuccessful"] = "transferred " + amount + " successfully";
                                responseTask = client.PostAsJsonAsync("BankAPI/api/Transaction/insertTransaction?SourceAccount=" + accountNo + "&destinationAccount=" + destinationAccountNo + "&amt=" + amount + "&type="+"FundTransfer" + "&comment=" + comment, amount);
                                responseTask.Wait();
                                var result1 = responseTask.Result;
                                int balance = amt - amount;
                                responseTask = client.GetAsync("BankAPI/api/Customer/getAccountTypeOfGivenAmount?amount=" + balance);
                                responseTask.Wait();
                                var result2 = responseTask.Result;
                                var readTask2 = result2.Content.ReadAsAsync<accountTypeDetailViewModel>();
                                readTask2.Wait();
                                accountTypeDetailViewModel cmedal = readTask2.Result;
                                Session["medal"] = cmedal.accountType;

                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                            }
                        }
                        
                        
                        ModelState.Clear();
                    }
                    else
                    {
                        Session["SameAccountFundTransferError"] = "Insufficient Amount";
                    }
                }
                else
                {
                    Session["SameAccountFundTransferError"] = "Destination Account not found";
                }
            }
            catch (Exception exp)
            {
                ViewBag.Error = "Exception " + exp;
            }
            return View();
        }



        public ActionResult MiniStatement()
        {
            Session["CustomStatementEmpty"] = "";
            Session["CustomStatementSuccess"] = "";
            long accountNo = long.Parse((Session["accountNumber"]).ToString());
            List<transactionDetailViewModel> obj = new List<transactionDetailViewModel>();
            List<transactionDetailViewModel> transactions = new List<transactionDetailViewModel>();
            using (client)
            {
                client.BaseAddress = new Uri("http://localhost:80");
                //HTTP GET
                var responseTask = client.GetAsync("BankAPI/api/Customer/GenerateMiniStatement?accountNo=" + accountNo);
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<List<transactionDetailViewModel>>();
                    readTask.Wait();
                    obj = readTask.Result;

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            transactions = obj;
            return View(transactions);
        }



        [HttpPost]
        public ActionResult MiniStatement(DateTime fromDate, DateTime toDate)
        {
            Session["CustomStatementEmpty"] = "";
            Session["CustomStatementSuccess"] = "";
            List<transactionDetailViewModel> obj = new List<transactionDetailViewModel>();
            List<transactionDetailViewModel> transactions = new List<transactionDetailViewModel>();
            long accountNo = Int32.Parse((Session["accountNumber"].ToString()));
            using (client)
            {
                client.BaseAddress = new Uri("http://localhost:80");
                //HTTP GET
                var responseTask = client.GetAsync("BankAPI/api/Customer/customstatement?acc=" + accountNo+"&start="+fromDate+"&end="+toDate);
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<List<transactionDetailViewModel>>();
                    readTask.Wait();
                    obj = readTask.Result;

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            transactions = obj;
            if (transactions.Count == 0)
            {
                Session["CustomStatementEmpty"] = "No transactions found from " + fromDate + " to " + toDate;
                return View();
            }
            else
            {
                Session["CustomStatementSuccess"] = "Transactions  from " + fromDate + " to " + toDate;
            }
            ModelState.Clear();
            return View(transactions);
        }

        public ActionResult ChangePassword()
        {
            Session["SucessChangePassword"] = "";
            Session["ErrorChangePassword"] = "";
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string oldPassword, string newPassword1, string newPassword2)
        {
            Session["SucessChangePassword"] = "";
            Session["ErrorChangePassword"] = "";
            string obj;
            using (client)
            {
                client.BaseAddress = new Uri("http://localhost:80");
                //HTTP GET
                var responseTask = client.PostAsJsonAsync("BankAPI/api/Customer/changePassword?oldPassword=" + oldPassword + "&newPassword1=" + newPassword1 + "&newPassword2=" + newPassword2 + "&loginId=" + Session["emailId"].ToString(),1);
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<string>();
                    readTask.Wait();
                    obj = readTask.Result;
                    Session["SucessChangePassword"] = obj;
                }
                else
                {
                    //ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    var readTask = result.Content.ReadAsAsync<string>();
                    readTask.Wait();
                    obj = readTask.Result;
                    Session["ErrorChangePassword"] = obj;
                }
            }
                ModelState.Clear();
            return View();
        }

        public ActionResult CustomStatement()
        {
            return View();
        }
    }
}