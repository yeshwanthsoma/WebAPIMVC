using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer;
using System.Net.Http;
using MVCPractice.Models;

namespace MVCPractice.Controllers
{
    public class ManagerController : Controller
    {
        //
        // GET: /Manager/
        HttpClient client = new HttpClient();
        public ActionResult Index()
        {
            List<userDetailViewModel> customers = new List<userDetailViewModel>();
            try
            {
                string emailId = Convert.ToString(Session["emailId"]);
                using (var client = new HttpClient())
                {
                    int userId;
                    string error;
                    client.BaseAddress = new Uri("http://153.59.21.37:8090/api/Manager/");
                    var responseTask = client.GetAsync("getUserIdFromEmailId?emailId=" + emailId);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<int>();
                        readTask.Wait();
                        userId = readTask.Result;
                        //Another Web Api call
                        responseTask = client.GetAsync("getCustomersByManagerId?managerId=" + userId);
                        responseTask.Wait();
                        result = responseTask.Result;

                        if (result.IsSuccessStatusCode)
                        {
                            var readTask2 = result.Content.ReadAsAsync<List<userDetailViewModel>>();
                            readTask2.Wait();
                            customers = readTask2.Result;
                        }
                        else
                        {
                            throw new Exception("Exception occured while getting customers");
                        }

                    }
                    else //if emailId is not existing in the table
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        error = readTask.Result;
                        //ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            }
            return View(customers);
        }


        public ActionResult Withdraw()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Withdraw(accountDetailViewModel account)
        {
            string emailId = Convert.ToString(Session["emailId"]);
            string resultMessage = ""; int userId;
            try
            {

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://153.59.21.37:8090/api/");
                    var postTask = client.PostAsJsonAsync<accountDetailViewModel>("Manager/withdrawAmount", account);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        // client.BaseAddress = new Uri("http://153.59.21.37:8090/api/");
                        var responseTask = client.GetAsync("Manager/getUserIdFromEmailId?emailId=" + emailId);
                        responseTask.Wait();
                        var result2 = responseTask.Result;
                        if (result2.IsSuccessStatusCode)
                        {
                            var readTask = result2.Content.ReadAsAsync<int>();
                            readTask.Wait();
                            userId = readTask.Result;
                            postTask = client.PostAsJsonAsync<transactionDetailViewModel>("Transaction/insertTransaction",
                                                                new transactionDetailViewModel
                                                                {
                                                                    transactionType = "Withdraw",
                                                                    sourceAccountNumber = account.accountNumber,
                                                                    destinationAccountNumber = account.accountNumber,
                                                                    transactionAmount = (int)account.accountBalance,
                                                                    transactionAuthorizedBy = userId,
                                                                    comments = "Withdrawn"
                                                                });
                            postTask.Wait();
                            result = postTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                resultMessage = "Withdrawn " + account.accountBalance + " successfully";
                            }
                            else
                            {
                                resultMessage = "Exception Occured";
                            }
                        }
                        else
                        {
                            resultMessage = "error wyhile getting userId from Email";
                        }

                    }
                    else
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        resultMessage = readTask.Result;
                        //ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                ViewBag.result = resultMessage;

            }

            catch (Exception exp)
            {
                ViewBag.Error = "Exception " + exp;
            }
            return View();

        }

        public ActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Deposit(accountDetailViewModel account)
        {
            string emailId = Convert.ToString(Session["emailId"]);
            string resultMessage = ""; int userId;
            try
            {

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://153.59.21.37:8090/api/");
                    var postTask = client.PostAsJsonAsync<accountDetailViewModel>("Manager/depositAmount", account);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        // client.BaseAddress = new Uri("http://153.59.21.37:8090/api/");
                        var responseTask = client.GetAsync("Manager/getUserIdFromEmailId?emailId=" + emailId);
                        responseTask.Wait();
                        var result2 = responseTask.Result;
                        if (result2.IsSuccessStatusCode)
                        {
                            var readTask = result2.Content.ReadAsAsync<int>();
                            readTask.Wait();
                            userId = readTask.Result;
                            postTask = client.PostAsJsonAsync<transactionDetailViewModel>("Transaction/insertTransaction",
                                                                new transactionDetailViewModel
                                                                {
                                                                    transactionType = "Deposit",
                                                                    sourceAccountNumber = account.accountNumber,
                                                                    destinationAccountNumber = account.accountNumber,
                                                                    transactionAmount = (int)account.accountBalance,
                                                                    transactionAuthorizedBy = userId,
                                                                    comments = "Deposited"
                                                                });
                            postTask.Wait();
                            result = postTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                resultMessage = "Deposited " + account.accountBalance + " successfully";
                            }
                            else
                            {
                                resultMessage = "Exception Occured";
                            }
                        }
                        else
                        {
                            resultMessage = "error wyhile getting userId from Email";
                        }

                    }
                    else
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        resultMessage = readTask.Result;
                        //ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                ViewBag.result = resultMessage;

            }

            catch (Exception exp)
            {
                ViewBag.Error = "Exception " + exp;
            }
            return View();

        }



        public ActionResult Account()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Account(int customerId)
        {

            List<accountDetailViewModel> obj = new List<accountDetailViewModel>();
            //BankEntities2 dbContext = new BankEntities2();
            Session["customerId"] = customerId;
            string loginId = Session["emailId"].ToString();
            using (client)
            {
                client.BaseAddress = new Uri("http://localhost:80");
                //HTTP GET
                var responseTask = client.GetAsync("BankAPI/api/Manager/getAllAccountsOfBranchCustomer?userId=" + customerId + "&loginId=" + loginId);
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
                    var readTask = result.Content.ReadAsAsync<string>();
                    readTask.Wait();
                    string msg = readTask.Result;
                    ViewBag.msg = msg;
                    return View("Account");
                }
            }

            return View("Account", obj);
        }

        public ActionResult addAccount()
        {

            return View();
        }

        public ActionResult deleteAccount()
        {
            ManagerClass obj = new ManagerClass();
            long selectedAccount = long.Parse(Session["EditedAccount"].ToString());
            obj.DeleteAccount(selectedAccount);
            return View("Account");

        }
        [HttpPost]
        public ActionResult addAccount(String AccountType, String Status, int AmountTextBox)
        {
            ManagerClass obj = new ManagerClass();
            String[] EnteredDatails = new String[7];
            EnteredDatails[0] = Session["customerId"].ToString();
            EnteredDatails[1] = AccountType;
            EnteredDatails[2] = Session["Today"].ToString();
            EnteredDatails[3] = Status;
            EnteredDatails[4] = Session["Today"].ToString();
            EnteredDatails[5] = "";
            EnteredDatails[6] = AmountTextBox.ToString();
            obj.AddAccount(EnteredDatails);

            return View();
        }
        [HttpPost]
        public ActionResult editAccount(long selectedAccount)
        {
            BankEntities2 dbContext = new BankEntities2();

            Session["EditedAccount"] = selectedAccount;
            ManagerClass obj = new ManagerClass();
            Account acc = dbContext.Accounts.Single(x => x.accountNo == selectedAccount);
            return View(acc);
        }
        [HttpPost]
        public ActionResult editAccounts(String AccountType, String Status, int amount)
        {
            ManagerClass obj = new ManagerClass();
            Account acc = new Account();
            acc.accountNo = long.Parse(Session["EditedAccount"].ToString());
            acc.accountType = AccountType;
            acc.status = Status;
            acc.dateOfEdited = Session["Today"].ToString();
            acc.ClosingDate = "";
            acc.amount = amount;
            obj.EditAccount(acc);

            return View("Account");
        }

        public ActionResult ManageCustomer()
        {
            if (TempData["deleteCustomer"] != null)
                ViewBag.deleteCustomer = TempData["deleteCustomer"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult ManageCustomer(int userId)
        {

            string managerEmailId = Session["emailId"].ToString();
            userDetailViewModel customer = new userDetailViewModel();
            Session["CustomerId"] = userId;

            using (var client = new HttpClient())
            {
                int managerUserId;

                client.BaseAddress = new Uri("http://153.59.21.37:8090/api/Manager/");
                var responseTask = client.GetAsync("getUserIdFromEmailId?emailId=" + managerEmailId);
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<int>();
                    readTask.Wait();
                    managerUserId = readTask.Result;
                    //Another Web Api call
                    responseTask = client.GetAsync("getUserDetailsFromUserId?userId=" + userId);
                    responseTask.Wait();
                    result = responseTask.Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var readTask2 = result.Content.ReadAsAsync<userDetailViewModel>();
                        readTask2.Wait();
                        customer = readTask2.Result;
                        if (customer.managerId == managerUserId)
                        {
                            return View(customer);
                        }
                        else
                        {
                            ViewBag.Error = "You cannot access this customer";
                        }
                    }
                    else
                    {
                        ViewBag.Error = "No customer Exists";
                    }

                }
                else //if emailId is not existing in the table
                {
                    var readTask = result.Content.ReadAsAsync<string>();
                    readTask.Wait();
                    ViewBag.Error = readTask.Result;
                    //ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            ViewBag.customerId = userId;
            return View();
        }

        public ActionResult ShowAllCustomers()
        {
            List<userDetailViewModel> customers = new List<userDetailViewModel>();
            try
            {
                string emailId = Convert.ToString(Session["emailId"]);
                using (var client = new HttpClient())
                {
                    int userId;
                    string error;
                    client.BaseAddress = new Uri("http://153.59.21.37:8090/api/Manager/");
                    var responseTask = client.GetAsync("getUserIdFromEmailId?emailId=" + emailId);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<int>();
                        readTask.Wait();
                        userId = readTask.Result;
                        //Another Web Api call
                        responseTask = client.GetAsync("getCustomersByManagerId?managerId=" + userId);
                        responseTask.Wait();
                        result = responseTask.Result;

                        if (result.IsSuccessStatusCode)
                        {
                            var readTask2 = result.Content.ReadAsAsync<List<userDetailViewModel>>();
                            readTask2.Wait();
                            customers = readTask2.Result;
                        }
                        else
                        {
                            throw new Exception("Exception occured while getting customers");
                        }

                    }
                    else //if emailId is not existing in the table
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        error = readTask.Result;
                        //ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            }
            return View(customers);

        }

        public ActionResult AddCustomer()
        {
            return View();
        }

        public string Encrypt(string strEncrypted)
        {
            byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(strEncrypted);
            string encrypted = Convert.ToBase64String(b);
            return encrypted;
        }

        [HttpPost]
        public ActionResult AddCustomer(Customer custObj)
        {
            try
            {

                custObj.userId = custObj.email;
                custObj.createdDate = DateTime.Now.ToShortDateString();
                custObj.editedDate = DateTime.Now.ToShortDateString();
                //custObj.type = "Bronze";
                string managerId = Session["emailId"].ToString();
                BankEntities2 dbContext = new BankEntities2();
                Manager mgrDetails = dbContext.Managers.Single(x => x.userId == managerId);
                custObj.managerId = mgrDetails.managerId;
                custObj.branchId = mgrDetails.branchId;

                ManagerClass classcustObj = new ManagerClass();
                classcustObj.addLogin(custObj.email, Encrypt("123456"), "Customer");
                int rows_affected = classcustObj.addCustomer(custObj);
                if (rows_affected == 0)
                {
                    ViewBag.Error = "Customer Not added";
                }
                else
                {
                    ModelState.Clear();
                    ViewBag.Success = "Customer Added";

                }


            }
            catch (Exception ex)
            {
                ViewBag.Error = "Exception";
            }
            return View();

        }

        public ActionResult EditCustomer()
        {
            BankEntities2 dbContext = new BankEntities2();
            int custId = int.Parse(Session["CustomerId"].ToString());
            Customer obj = dbContext.Customers.Single(x => x.customerId == custId);
            return View(obj);
        }

        [HttpPost]
        public ActionResult EditCustomer(Customer custObj)
        {
            try
            {
                custObj.customerId = int.Parse(Session["CustomerId"].ToString());
                custObj.editedDate = DateTime.Now.ToShortDateString();
                BankEntities2 dbContext = new BankEntities2();
                ManagerClass classcustObj = new ManagerClass();


                int rows_affected = classcustObj.updateCustomer(custObj);
                if (rows_affected == 0)
                {
                    ViewBag.Error = "Customer Not updated";
                }
                else
                {
                    ModelState.Clear();
                    ViewBag.Success = "Customer Editted successfully";

                }


            }
            catch (Exception ex)
            {
                ViewBag.Error = "Exception";
            }
            return View();
        }

        public ActionResult DeleteCustomer()
        {
            ManagerClass classCustObj = new ManagerClass();
            int rows_affected = classCustObj.deleteCustomer(int.Parse(Session["CustomerId"].ToString()));
            if (rows_affected == 0)
            {
                TempData.Add("deleteCustomer", "Error while deleting customer");
                //ViewBag.deleteCustomer="Error while deleting customer";
            }
            else
            {
                TempData.Add("deleteCustomer", "Customer Deleted successfully");
                ViewBag.deleteCustomer = "Customer Deleted successfully";
            }
            return RedirectToAction("ManageCustomer");
        }

    }
}
