﻿using AjaxControlToolkit;
using JG_Prospect.BLL;
using JG_Prospect.Common.modal;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace JG_Prospect.Sr_App.Controls
{
    public partial class AddProductLinesControl : System.Web.UI.UserControl
    {
        public string ImageDataSourceKey
        {
            get
            {
                return "ImageDataSource_" + this.ID;
            }
        }
        public List<CustomerLocationPic> CustomerLocationPicturesList
        {
            get
            {
                return ViewState[ImageDataSourceKey] == null ? null : (List<CustomerLocationPic>)ViewState[ImageDataSourceKey];
            }
            set
            {
                ViewState[ImageDataSourceKey] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            BindProductsDDL();
            var productLineProductName = Request.Form[ddlProductLines.UniqueID];
            var selectedProduct = ddlProductLines.Items.FindByValue(productLineProductName);
            if (selectedProduct != null)
            {
                selectedProduct.Selected = true;
            }
        }

        private void BindProductsDDL()
        {
            DataSet ds = UserBLL.Instance.GetAllProducts();
            ddlProductLines.DataSource = ds;
            ddlProductLines.DataTextField = "ProductName";
            ddlProductLines.DataValueField = "ProductId";
            ddlProductLines.DataBind();
            ddlProductLines.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0)"));
        }
        protected void ddlProductLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProposalTerms(ddlProductLines.SelectedValue.Trim());
        }
        private void LoadProposalTerms(string selectedProductLineValue)
        {
            DataSet dsTerms;
            var ProductTypeId = Convert.ToInt16(selectedProductLineValue);

            if (ProductTypeId != 0)
            {
                string productName = UserBLL.Instance.GetProductNameByProductId(ProductTypeId);
                string ProposalTerm = string.Empty;
                dsTerms = new_customerBLL.Instance.GetProposalTerm(Convert.ToInt32(ProductTypeId));
                if (dsTerms.Tables.Count > 0)
                {
                    if (dsTerms.Tables[0].Rows.Count > 0)
                    {
                        if (Convert.ToString(dsTerms.Tables[0].Rows[0][1]) != "")
                        {
                            ProposalTerm = Convert.ToString(dsTerms.Tables[0].Rows[0][1]);
                            ProposalTerm = Regex.Replace(ProposalTerm, "<.*?>|&.*?;", string.Empty);
                            txtProposalTerm.Text = ProposalTerm;
                        }
                    }
                }
            }
        }

        protected void ajaxFileUpload_UploadedComplete(object sender, AsyncFileUploadEventArgs e)
        {
            if (true)//ValidateImageUpload(Path.GetFileName(ajaxFileUpload.FileName)))
            {
                string imageName = Path.GetFileName(ajaxFileUpload.FileName);
                string tempImageName = Guid.NewGuid() + "-" + imageName;
                ajaxFileUpload.SaveAs(Server.MapPath("~/CustomerDocs/LocationPics/" + tempImageName));

                tempImageName = "../../CustomerDocs/LocationPics/" + tempImageName;

                if (CustomerLocationPicturesList == null)
                {
                    CustomerLocationPicturesList = new List<CustomerLocationPic>();
                }

                int srNo = CustomerLocationPicturesList.Count + 1;
                CustomerLocationPicturesList.Add(new CustomerLocationPic { RowSerialNo = srNo, LocationPicture = tempImageName });

                hidCount.Value = CustomerLocationPicturesList.Count == 0 ? string.Empty : CustomerLocationPicturesList.Count.ToString();
                gvCategory.DataSource = CustomerLocationPicturesList;
                gvCategory.DataBind();
            }
        }
        protected void btnImageUploadClick_Click(object sender, EventArgs e)
        {
            //This event is here just to update the update panel which refreshes the location images grid.
        }

        protected void gvCategory_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteRec")
            {
                int Id = Convert.ToInt32(e.CommandArgument.ToString());
                List<CustomerLocationPic> pics = (List<CustomerLocationPic>)ViewState[ImageDataSourceKey];
                pics.Remove(pics.FirstOrDefault(id => id.RowSerialNo == Id));
                ViewState[ImageDataSourceKey] = pics;
                hidCount.Value = "";
                hidCount.Value = pics.Count.ToString();
                gvCategory.DataSource = pics;
                gvCategory.DataBind();
            }
        }

        protected void gvCategory_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CustomerLocationPic dr = (CustomerLocationPic)e.Row.DataItem;
                string strImage = dr.LocationPicture;
                //((Image)(e.Row.FindControl("imglocation"))).ImageUrl = "~/CustomerDocs/LocationPics/" + strImage;
            }
        }

        protected void gvCategory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCategory.PageIndex = e.NewPageIndex;
            gvCategory.DataSource = ViewState[ImageDataSourceKey];
            gvCategory.DataBind();
        }

    }
}