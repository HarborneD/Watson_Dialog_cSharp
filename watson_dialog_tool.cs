using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Watson_Dialog.Helpers
{
    class watson_dialog_tool
    {
        public string base_url { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public string credentials_string { get; set; }

        public string version { get; set; }

        public string dialog_id { get; set; }

        public int conversation_id { get; set; }
        public int client_id { get; set; }

        public Dictionary<string,string> values { get; set; }

        public watson_dialog_tool()
        {
            this.construct_base_url();

            this.username = ConfigurationManager.AppSettings["WatsonUsername"];
            this.password = ConfigurationManager.AppSettings["WatsonPassword"];
            this.update_credentials_string();

            this.values = new Dictionary<string, string>();
        }

        private void construct_base_url()
        {
            this.version = ConfigurationManager.AppSettings["WatsonVersion"];
            this.base_url = ConfigurationManager.AppSettings["WatsonPath"] + "/" + this.version + "/dialogs";
        }

        public void update_credentials_string()
        {
            this.credentials_string = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", this.username, this.password)));
        }

        public watson_dialog_API_Reply make_request(RequestType r_type = RequestType.GET, string url_extension = "", string filename = "", byte[] bytes = null)
        {
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", this.credentials_string);


                if (!string.IsNullOrEmpty(filename))
                {
                    foreach (var keyValuePair in this.values)
                    {
                        formData.Add(new StringContent(keyValuePair.Value), string.Format("\"{0}\"", keyValuePair.Key));
                    }

                    if ((!string.IsNullOrEmpty(filename)) && bytes != null)
                    {
                        formData.Add(new ByteArrayContent(bytes), '"' + "file" + '"', "\"@" + filename + '"');
                    }
                }

                HttpResponseMessage response = null;
                switch (r_type)
                { 
                    case RequestType.POST:


                        if (string.IsNullOrEmpty(filename))
                        {
                            var data = new FormUrlEncodedContent(this.values);
                            response = client.PostAsync(this.base_url + url_extension, data).Result;
                        }
                        else
                        {
                            response = client.PostAsync(this.base_url + url_extension, formData).Result;
                        }

                        
                        break;

                    case RequestType.PUT:

                        response = client.PutAsync(this.base_url + url_extension, formData).Result;
                        break;

                    case RequestType.DELETE:

                        response = client.DeleteAsync(this.base_url + url_extension).Result;
                        break;
                
                    default:
                        response = client.GetAsync(this.base_url + url_extension).Result;
                        break;
                }

                var result = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    
                    
                    return new watson_dialog_API_Reply() { success = false, msg = response.ReasonPhrase };
                }
                else
                {
                    return new watson_dialog_API_Reply() { success = true,  response_json = result };
                }

               

            }
        }

        #region dialogs

        public watson_dialog_API_Reply get_dialogs()
        {          
            return this.make_request();
        }

        public watson_dialog_API_Reply create_dialog(string name,string filename, byte[] bytes)
        {
            this.values.Add("name", name);
            return this.make_request(RequestType.POST,"",filename,bytes);
        }

        public watson_dialog_API_Reply update_dialog(string dialog_id,string filename, byte[] bytes)
        {
            this.dialog_id = dialog_id;

            return this.make_request(RequestType.PUT,"/" + this.dialog_id, filename, bytes);
        }

        public watson_dialog_API_Reply delete_dialog(string dialog_id)
        {
            this.dialog_id = dialog_id;

            return this.make_request(RequestType.DELETE, "/" + this.dialog_id);
        }
        #endregion dialogs

        //conversations
        public void get_convo_history()
        {

        }

        public watson_dialog_API_Reply send_message(string dialog_id,string message = "hi", int conversation_id=0,int client_id =0)
        {
            this.dialog_id = dialog_id;

            if(conversation_id > 0 && client_id > 0)
            {
                this.conversation_id = conversation_id;
                this.client_id = client_id;

                this.values.Add("conversation_id", this.conversation_id.ToString());
                this.values.Add("client_id", this.client_id.ToString());

            }

            this.values.Add("input", message);
            
            return make_request(RequestType.POST,"/" + this.dialog_id + "/conversation");
        }

        public void get_client_vars()
        {

        }

  

    }

    public class watson_dialog_API_Reply
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public string response_json { get; set; } 
    }

    public class watson_dialog_dialog
    {
        public string name { get; set; }
        public string dialog_id { get; set; }
    }

    public class watson_dialog_response
    {
        public List<string> response { get; set; }

        public string input { get; set; }
        public int conversation_id { get; set; }
        public float confidence { get; set; }
        public int client_id { get; set; }
    }

    public enum RequestType { GET, POST, PUT,DELETE };

    public class watson_dialog_Conversation_response
    {
        public string response { get; set; }

        public watson_dialog_Conversation_response()
        {
            response = "";
        }
    }



}
