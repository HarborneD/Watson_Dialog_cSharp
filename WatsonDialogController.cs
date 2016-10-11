using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Watson_Dialog.Helpers;

namespace Watson_Dialog.Controllers
{
    public class WatsonDialogController : Controller
    {
        // GET: WatsonDialog
        public ActionResult Index()
        {
            return View();
        }
        //HttpPostedFileBase

        //Dialogs
        public ActionResult Dialogs(string msg ="")
        {
            watson_dialog_tool dialog_tool = new watson_dialog_tool();

            watson_dialog_API_Reply dialogs = dialog_tool.get_dialogs();

            List<watson_dialog_dialog> dialog_list = new List<watson_dialog_dialog>();

            if (dialogs.success)
            {
                dynamic json_dialog_list = JObject.Parse(dialogs.response_json);

                foreach(Object o in json_dialog_list.dialogs)
                {
                    dialog_list.Add(JsonConvert.DeserializeObject<watson_dialog_dialog>(o.ToString()));
                }
            }

            ViewBag.msg = msg;
            return View(dialog_list);
          
        }

        public ActionResult CreateDialog()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDialog([Bind(Include = "name")] watson_dialog_dialog dialog, HttpPostedFileBase XML)
        {

            string fileName = Path.GetFileName(XML.FileName);

            byte[] bytes = null;

            using (Stream inputStream = XML.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }

                bytes = memoryStream.ToArray();
            }


            watson_dialog_tool dialog_tool = new watson_dialog_tool();

            watson_dialog_API_Reply create_response = dialog_tool.create_dialog(dialog.name, fileName,bytes);

            string create_message = "";
            if (create_response.success)
            {
                create_message = dialog.name + " was created successfully.";
            }
            else
            {
                create_message = "There was an error create dialog " + dialog.name + " - " + create_response.msg;
            }

            return RedirectToAction("Dialogs", new { msg = create_message });

         
        }



        public ActionResult UpdateDialog(string dialog_id, string name)
        {
            watson_dialog_dialog dialog = new watson_dialog_dialog() { dialog_id = dialog_id, name = name };

            return View(dialog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDialog([Bind(Include = "dialog_id")] watson_dialog_dialog dialog, HttpPostedFileBase XML)
        {

            string fileName = Path.GetFileName(XML.FileName);

            byte[] bytes = null;

            using (Stream inputStream = XML.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }

                bytes = memoryStream.ToArray();
            }


            watson_dialog_tool dialog_tool = new watson_dialog_tool();
            

            watson_dialog_API_Reply update_response = dialog_tool.update_dialog(dialog.dialog_id, fileName, bytes);

            string update_message = "";
            if (update_response.success)
            {
                update_message = dialog.dialog_id + " was updated successfully.";
            }
            else
            {
                update_message = "There was an error updating dialog "+dialog.dialog_id + " - " + update_response.msg;
            }
            
            return RedirectToAction("Dialogs", new {msg = update_message });
        }

        public ActionResult DeleteDialog(string dialog_id, string name)
        {
            watson_dialog_dialog dialog = new watson_dialog_dialog() { dialog_id = dialog_id, name = name };

            return View(dialog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDialog([Bind(Include = "dialog_id")] watson_dialog_dialog dialog)
        {

          watson_dialog_tool dialog_tool = new watson_dialog_tool();


          watson_dialog_API_Reply delete_response = dialog_tool.delete_dialog(dialog.dialog_id);

            string update_message = "";
            if (delete_response.success)
            {
                update_message = dialog.dialog_id + " was deleted successfully.";
            }
            else
            {
                update_message = "There was an error deleting dialog " + dialog.dialog_id + " - " +delete_response.msg;
            }

            return RedirectToAction("Dialogs", new { msg = update_message });
        }

        //conversations

        public ActionResult CreateConversation(string dialog_id)
        {
            watson_dialog_tool dialog_tool = new watson_dialog_tool();

            watson_dialog_API_Reply create_response = dialog_tool.send_message(dialog_id);

            watson_dialog_Conversation_response response = new watson_dialog_Conversation_response();

            if (create_response.success)
            {
                dynamic json_response = JObject.Parse(create_response.response_json);

                Session["dialog_id"]= dialog_id;
                Session["conversation_id"] = json_response.conversation_id;
                Session["client_id"] = json_response.client_id;

                string[] response_list = JsonConvert.DeserializeObject<string[]>(json_response.response.ToString());

                foreach (string s in response_list)
                {
                    if(response.response.Length > 0)
                    {
                        response.response += "/n";
                    }
                    response.response += s.ToString();
                }
                
            }
            else
            {
                response.response = "<There was an error sending your message>";
            }

            return RedirectToAction("ViewConversation" ,new { OpeningMessage = response.response });

        }

        public ActionResult ViewConversation(string OpeningMessage)
        {
            ViewBag.OpeningMessage = OpeningMessage;
            return View();
        }


        public JsonResult SendMessage(string message)
        {
            
            watson_dialog_tool dialog_tool = new watson_dialog_tool();

            watson_dialog_API_Reply send_response = dialog_tool.send_message(Session["dialog_id"].ToString(),'"'+message+'"',Convert.ToInt32(Session["conversation_id"]),Convert.ToInt32(Session["client_id"]));

            watson_dialog_Conversation_response response = new watson_dialog_Conversation_response();

            if (send_response.success)
            {

                dynamic json_response = JObject.Parse(send_response.response_json);

                 string[] response_list = JsonConvert.DeserializeObject<string[]>(json_response.response.ToString());

                foreach (string s in response_list)
                {
                    if (response.response.Length > 0)
                    {
                        response.response += "/n";
                    }
                    response.response += s.ToString();
                }
            }
            else
            {
                response.response = "<There was an error sending your message>";
            }




            return Json(response, JsonRequestBehavior.AllowGet);
        }



    }
}