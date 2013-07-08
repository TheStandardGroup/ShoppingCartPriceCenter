using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pageflex.Interfaces.Storefront;
using PageflexServices;
using System.Web;
using System.Web.UI;

namespace ShoppingCartPriceCenter
{
    public class ShoppingCartPriceCenter: SXIExtension
    {
        public override string UniqueName
        {
            get
            {
                return "ShoppingCartPriceCenter.standardgroup.com";
            }
        }
        public override string DisplayName
        {
            get
            {
                return "TSG: Shopping Cart Price Center";
            }
        }

        public override int GetConfigurationHtml(KeyValuePair[] parameters, out string HTML_configString) {
            if (parameters == null)
                HTML_configString = "<p>Each product in your storefront must have a PriceCenter field in the printing options.</p>";
            else
                HTML_configString = null;
            return 0;
        }

        public override int PageLoad(string pageBaseName, string eventName)
        {

            if ((pageBaseName == "usercontentshoppingcart_aspx") && (eventName == "Init"))
            {
                string url = HttpContext.Current.Request.Url.AbsoluteUri;
                string[] findVars = url.Split('?');
                var page = HttpContext.Current.CurrentHandler as Page;
                string userId = Storefront.GetValue("SystemProperty", "LoggedOnUserID", null);
                string[] docInCart = Storefront.GetListValue("UserListProperty", "DocumentsInShoppingCart", userId);
                string[] docOnHold = Storefront.GetListValue("UserListProperty", "DocumentsOnHold", userId);
                string[] CostCenters = new String[docInCart.Length];
                for (int i = 0; i < docInCart.Length; i++) {
                    CostCenters[i] = Storefront.GetValue("PrintingField", "PriceCenter", docInCart[i]);
                }
                //string onHold = "OnHold";
                if (findVars.Length > 1 && findVars[1].Equals("CC"))
                {
                    string [] myVars = findVars[2].Split(',');
                    int docId = Convert.ToInt32(myVars[0]);
                    Storefront.SetValue("PrintingField", "PriceCenter", docInCart[docId], myVars[1]);
                    HttpContext.Current.Response.Redirect(findVars[0]);
                }
                if (findVars.Length > 1 && findVars[1].Equals("failedCC")) {
                    HttpContext.Current.Response.Redirect(findVars[0],true);
                }
                string js = createJavaScript(userId, CostCenters);
                page.ClientScript.RegisterStartupScript(this.GetType(), "Add_Cost_Center", js);
            }

            return eSuccess;
        }

        private string createJavaScript(string userId,string[] CostCenters) {
            string js = "<script type='text/javascript' src='jSINI.js'></script>";
            js += "<style type = 'text/css'>";
            js += "a.disabledForButtons {";
            js += " opacity: 0.5;";
            js += " pointer-events: none;";
            js += " cursor: default;";
            js += "}";
            js += "</style>";

            js += "<script type = 'text/javascript'>";
            js += "Sys.WebForms.PageRequestManager.getInstance().add_endRequest(myJQueryRequestHandler);";
            js += "function myJQueryRequestHandler(sender,args){";
            js += "     priceCenterAddColToCart();}";
            js += "$(document).ready(function(){";
            js += "     priceCenterAddColAnotherToCart();";
            js += "});";
            js += "function priceCenterAddColAnotherToCart(){";
            js += "     var cc = CallSINIMethod('GetListValue', ['UserListProperty','DocumentsInShoppingCart'," + userId + "]);";
            js += "     $('#CartTable .itemTableHeader-Options').after('<td class=\"itemTableHeader itemTableHeader-Id\">Cost Center</td>');";
            js += "     $('#CartTable .itemTable-Id').each(function(i) {";
            //js += "         var myVal = CallSINIMethod('GetValue',['PrintingField','PriceCenter',cc[i]]);";
            js += "         var c=0;";
            js += "         var id = $(this).text().replace(/‑/, \"\");";
            js += "         id = id.replace(\"‑\",\"\");";
            js += "         var myIdx = 0;";
            js += "         var found = false;";
            js += "         while(c<cc.length && !found){";
            js += "             var myName = CallSINIMethod('GetValue',['DocumentProperty','ExternalID',cc[c]]).replace(\"-\",\"\");";
            js += "             myName = myName.replace(\"-\",\"\");";
            //js += "             alert('myName ='+ myName + '   id = '+id);";
            js += "             if(id == myName){";
            js += "                 myIdx = c;";
            js += "                 found = true;}";
            js += "             c += 1;";
            js += "         }";
            js += "         var myVal = CallSINIMethod('GetValue',['PrintingField','PriceCenter',cc[myIdx]]);";
            
            js += "         if(!myVal){myVal = '';}";
            js += "         $(this).parent().parent().siblings('.itemTable-Options').after('<td class=\"itemTable itemTable-CostCenter\"><input type=\"text\" class=\"tBox\" name=\"'+myIdx+'\" value=\"'+myVal+'\" size=\"5\"><br>";
            js += "                             </td>');});";
            js += "     $('.tBox').change(function() {";
            js += "         evalCostCenters();";
            //js += "         alert($(this).val());";
            js += "         var loc = document.location.toString();";
            js += "         loc += '?CC?'+ $(this).attr('name') +','+$(this).val();";
            //js += "         CallSINIMethod('SetValue',['PrintingField','PriceCenter',cc[parseInt($(this).attr('name'))],$(this).val()]);";
            js += "         document.location = loc;";
            js += "     });";
            js += "     evalCostCenters();";
            //js += "         $(this).after('<td class=\"itemTable itemTable-Id\">Test</td>');});";
            js += "}";
            //js += "function StorefrontValidatorHook(){";
            //js += "     return evalCostCenters();";
            //js += "}";
            js += "function evalCostCenters(){";
            js += "     var isOk = false;";
            js += "     $('.tBox').each(function(i){";
            js += "         if($(this).val() == \"\"){";
            js += "             $('.itemTable-CostCenter:eq('+i+')').append('<font color=\"red\">Required</font>');";
            js += "             isOk = false;";
            js += "         }else{";
            js += "             if(i == 0)";
            js += "                 isOk=true;";
            js += "         }";
            js += "     });";
            //js += "     alert(isOk);";
            js += "     if(!isOk){$('#ShoppingCart_CartPanel').append('<font color=\"red\">Cost Center Required</font>');";
            //js += "         var i = 0;";
            js += "         $('.stepControlLabel').each(function(i){";
            //js += "             alert(i);";
            js += "             if(i>2){";
            js += "                 $(this).addClass('disabledForButtons');}});";
            js += "     }";
            js += "     return isOk;";
            js += "}";
            //js += "function StorefrontEvaluateFieldsHook(){return evalCostCenter();}";
            js += "</script>";
            return js;
        }
    }
}
