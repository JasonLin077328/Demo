using AutoMapper;
using Demo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 清單頁面
        /// </summary>
        /// <param name="oEmployeeIndexViewModel"></param>
        /// <returns></returns>
        public ActionResult EmployeeIndex(EmployeeIndexViewModel oEmployeeIndexViewModel)
        {
            //查詢邏輯寫在 ViewModel 可讓 viewmodel 重複利用
            oEmployeeIndexViewModel.Search();
            return View(oEmployeeIndexViewModel);
        }
        /// <summary>
        /// 修改頁面 調出資料
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]       
        public ActionResult EmployeeEdit(int ID = 0)
        {
            EmployeeEditViewModel oEmployeeEditModel = null;
            try
            {
                //從資料庫拉取資料
                using (DemoEntities DemoEntities = new DemoEntities())
                {
                    oEmployeeEditModel = DemoEntities.Employee.Where(o => o.EmployeeID == ID).ProjectToSingleOrDefault<EmployeeEditViewModel>();
                }
                //如果無資料 表示新增
                if (oEmployeeEditModel==null)
                {
                    oEmployeeEditModel = new EmployeeEditViewModel();
                }
            }
            catch (Exception ex)
            {
                return View();
            }
            return View(oEmployeeEditModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeEdit(EmployeeEditViewModel oEmployeeEditModel)
        {
            try
            {
                //檢查model 是否合法
                if (ModelState.IsValid)
                {
                    //如果合法執行 新增或修改
                    oEmployeeEditModel.SaveOrInsert();
                    return RedirectToAction("EmployeeIndex");
                }
            }
            catch (Exception ex)
            {
                return View();
            }
            return View(oEmployeeEditModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(EmployeeEditViewModel oEmployeeEditModel)
        {
            try
            {
                oEmployeeEditModel.Delete();
                return RedirectToAction("EmployeeIndex");
            }
            catch (Exception e)
            {
                return RedirectToAction("EmployeeIndex");
            }
        }

    }
    /// <summary>
    /// 單筆資料 或編輯頁面的ViewModel
    /// </summary>
    public class EmployeeEditViewModel : IMapFrom<Employee>
    {
        [Key]
        public int EmployeeID { get; set; }
        [Display(Name = "姓名")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "地址")]
        [Required]
        public string Address { get; set; }

        /// <summary>
        /// 執行新增或修改
        /// </summary>
        public void SaveOrInsert()
        {
            
            using (DemoEntities DemoEntities = new DemoEntities())
            {
                //將ViewModel 轉換成 ＥntityＭodel
                Employee EEmployee = Mapper.Map<EmployeeEditViewModel, Employee>(this);
                //如果有 EmployeeID 表示修改
                if (this.EmployeeID!=0)
                {
                    DemoEntities.Entry(EEmployee).State = System.Data.Entity.EntityState.Modified;
                    DemoEntities.SaveChanges();
                }
                else
                {
                    //沒有 EmployeeID 表示新增
                    try
                    {
                        DemoEntities.Employee.Add(EEmployee);
                        DemoEntities.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                  
                }
            }
        }
        /// <summary>
        /// 執行刪除
        /// </summary>
        public void Delete()
        {
            using (DemoEntities DemoEntities = new DemoEntities())
            {
                Employee Employee = DemoEntities.Employee.Single(x => x.EmployeeID == this.EmployeeID);
                DemoEntities.Entry(Employee).State =System.Data.Entity.EntityState.Deleted;
                DemoEntities.SaveChanges();
            }
        }
    }
    /// <summary>
    /// 清單頁面的 ViewModel
    /// </summary>
    public class EmployeeIndexViewModel
    {
        /// <summary>
        /// 查詢條件
        /// </summary>
        [Display(Name = "員工編號")]
        public string Q_EmployeeID { get; set; }

        [Display(Name = "地址")]
        public string Q_Address { get; set; }

        public List<EmployeeEditViewModel> ListData = new List<EmployeeEditViewModel>();

        /// <summary>
        /// 查詢資料 by 此 ViewModel 查詢條件
        /// </summary>
        /// <returns></returns>
        public void Search()
        {
            using (DemoEntities DemoEntities = new DemoEntities())
            {
                //使用 IQueryable 確保資料尚未做查詢
                IQueryable<Employee> Result = null;

                Result = DemoEntities.Employee;
                //複合查詢
                if (!string.IsNullOrEmpty(Q_EmployeeID))
                {
                    Result = Result.Where(o => o.EmployeeID.ToString().Contains(Q_EmployeeID));
                }
                if (!string.IsNullOrEmpty(Q_Address))
                {
                    Result = Result.Where(o => o.Address.Contains(Q_Address));
                }
                //將資料由 List ＥntityＭodel 轉換為 List ViewModel
                ListData = Result.ProjectToList<EmployeeEditViewModel>();

            }
        }
    }
}