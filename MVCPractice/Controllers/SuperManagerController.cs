using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer;

namespace MVCPractice.Controllers
{
    public class SuperManagerController : Controller
    {

        public ActionResult Index()
        {
            BankEntities2 newObj = new BankEntities2();
            List<Manager> managers = newObj.Managers.ToList();
            if (Session["mgrId"] != null)
            {
                Session["mgrId"] = null;
                //BankEntities2 newObj = new BankEntities2();
                //List<Manager> managers = newObj.Managers.ToList();
                return View(managers);
               // return View();
            }
            return View(managers);
        }

        public ActionResult ManagersList()
        {
            BankEntities2 newObj = new BankEntities2();
            List<Manager> managers = newObj.Managers.ToList();
            return View(managers);
        }

        public ActionResult ManageManager()
        {
            return View();
        }

        [HttpPost]

        public ActionResult ManageManager(int managerId)
        {
            BankEntities2 dbContext = new BankEntities2();
            Manager managers = dbContext.Managers.Single(x => x.managerId == managerId);
            ViewBag.mgrId = managerId;
            return View(managers);
        }

        public ActionResult AddManager()
        {
            return View();
        }

         [HttpPost]
        public ActionResult AddManager(string name, string branch, string address, string phno, string mail)
        {

            try
            {

                SuperManagerClass obj = new SuperManagerClass();

                string res = obj.addManager(name, branch, address, phno, mail);
                ViewBag.result = res;

            }

            catch (Exception exp)
            {
                ViewBag.Error = "Exception " + exp;
            }
            return RedirectToAction("Index");
        }

        
        public ActionResult EditManager(int id)
         {
             ViewBag.mgrId = id;
             Session["mgrId"] = id;
             BankEntities2 newObj = new BankEntities2();
             try
             {
                 SuperManagerClass superManagerClass = new SuperManagerClass();
                 List<string> branchesList = superManagerClass.getNonAssignedBranches();
             }
             catch (Exception e)
             {
                 ViewBag.Error = "Exception " + e;
             }
             Manager obj = newObj.Managers.Single(x => x.managerId == id);
            return View(obj);
         }

        [HttpPost]
        public ActionResult EditManager(string managerName, string address, string phoneNo, string emailId, string branchId)
        {
            int mgrId =(int)Session["mgrId"];
            try 
            {
                SuperManagerClass obj = new SuperManagerClass();

                string res = obj.assignToZero(mgrId);
                //ViewBag.result = res;



                string resu = obj.editManager(mgrId, managerName, address, phoneNo, emailId, branchId);
                ViewBag.result1 = resu;

            }

            catch (Exception exp)
            {
                ViewBag.Error = "Exception " + exp;
            }
            return RedirectToAction("Index");
        }

        public ActionResult DeleteManager(int id)
        {
            ViewBag.mgrId = id;
            Session["mgrId"] = id;
            return View();
        }

        [HttpPost]

        public ActionResult DeleteManager()
        {
            int mgrId = (int)Session["mgrId"];
            try
            {

                SuperManagerClass obj = new SuperManagerClass();

                string res = obj.deleteManager(mgrId);
                ViewBag.result = res;

            }

            catch (Exception exp)
            {
                ViewBag.Error = "Exception " + exp;
            }
            return RedirectToAction("Index");
        }


    }
}
