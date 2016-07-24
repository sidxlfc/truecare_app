using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Xml;

namespace truecare_app
{
    [Activity(Label = "truecare_app", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private List<string> mItems;

        private ListView mListView;

        ISharedPreferences isp = Application.Context.GetSharedPreferences("XMLString", FileCreationMode.Private);

        ISharedPreferencesEditor ispe;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //initialization
            
            mListView = FindViewById<ListView>(Resource.Id.myListView);

            ArrayAdapter<string> adapter;
            //initialize the content string
            var content = "";

            //check if data has been downloaded before

            string test = isp.GetString("XMLData", String.Empty);

            Console.Out.WriteLine("test : " + test);

            if (test.Equals(String.Empty))
            {

                //making the connection and defining request properties
                var rxcui = "198440";
                var request = HttpWebRequest.Create(string.Format(@"http://rxnav.nlm.nih.gov/REST/RxTerms/rxcui/{0}/allinfo", rxcui));
                request.ContentType = "application/json";
                request.Method = "GET";

                //sending the request
                
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            Console.Out.WriteLine("Response contained empty body...");
                        }
                        else
                        {
                            Console.Out.WriteLine("Response Body: \r\n {0}", content);
                            ispe = isp.Edit();
                            ispe.PutString("XMLData", content);
                            ispe.Apply();
                        }

                        //Assert.NotNull(content);
                    }
                }

            }

            else
            {
                content = test;
            }

            //initialize the Items List
            mItems = new List<string>();

            //process the XML string

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);

            XmlNodeList displayName = xmlDoc.GetElementsByTagName("displayName");
            XmlNodeList synonym = xmlDoc.GetElementsByTagName("synonym");
            XmlNodeList fullName = xmlDoc.GetElementsByTagName("fullName");
            XmlNodeList strength = xmlDoc.GetElementsByTagName("strength");
            XmlNodeList route = xmlDoc.GetElementsByTagName("route");


            //main logic

            mItems.Add("Name of the medicine : " + displayName[0].InnerText);
            mItems.Add("Synonym : " + synonym[0].InnerText);
            mItems.Add("fullName : " + fullName[0].InnerText);
            mItems.Add("Strength : " + strength[0].InnerText);
            mItems.Add("Route : " + route[0].InnerText);

            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mItems);

            mListView.Adapter = adapter;
       }
    }
}