using System;
using DYMO.Label.Framework;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeDymoLib
{
    public class Dymo
    {
        public async Task<object> GetPrintersAsync(dynamic input)
        {
            Debug.WriteLine("NodeDymoLibrary: GetPrintersAsync() called");
            Task<object> getPrintersTask = GetPrinters(input);

            Debug.WriteLine("NodeDymoLibrary: GetPrintersAsync() pre await");
            object response = await getPrintersTask;


            return response;
        }


        public async Task<object> GetPrinters(dynamic input)
        {
            Debug.WriteLine("NodeDymoLibrary: GetPrinters() called");

            IPrinters temp = new Printers();
            return (object)temp.Cast<IPrinter>().ToList();
        }

        /*
		 *
		 *
		 *
		 *
		 */
        public async Task<object> PrintLabelsAsync(dynamic jobDetails)
        {
            Debug.WriteLine("NodeDymoLibrary: PrintLabelsAsync() called");
            Task<object> printLabelsTask = PrintLabels(jobDetails);

            Debug.WriteLine("NodeDymoLibrary: PrintLabelsAsync() pre await");
            object response = await printLabelsTask;


            return response;
        }


        public async Task<object> PrintLabels(dynamic jobDetails)
        {
            Debug.WriteLine("NodeDymoLibrary: PrintLabels() called");
            // Debug.WriteLine(jobDetails.printer);
            //
            // Make sure the required parts exist
            if (!PropertyExists(jobDetails, "printer"))
            {
                Debug.WriteLine("NodeDymoLibrary: No `printer` parameter");
                throw new System.ArgumentException("'printer' parameter must be defined", "original");
            }
            else if (!PropertyExists(jobDetails, "labels"))
            {
                Debug.WriteLine("NodeDymoLibrary: No `labels` parameter");
                throw new System.ArgumentException("'labels' parameter must be defined", "original");
            }
            else
            {
                Debug.WriteLine("NodeDymoLibrary: Required base parameters defined");
                //Debug.WriteLine(jobDetails.printer);
            }

            //
            // Setup the printer and this job
            //ILabelWriterPrinter printer = Framework.GetLabelWriterPrinters()[(String)jobDetails.printer] as ILabelWriterPrinter;
            IPrinter printer;
            try
            {
                printer = Framework.GetPrinters()[jobDetails.printer] as IPrinter;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("NodeDymoLibrary: unable to find printer");
                throw ex;
            }

            //
            // Set some settings for this printing
            ILabelWriterPrintParams printParams = new LabelWriterPrintParams();
            printParams.PrintQuality = LabelWriterPrintQuality.BarcodeAndGraphics;
            printParams.JobTitle = "Dymo Labels";
            printParams.Copies = (int)1;
            if (PropertyExists(jobDetails, "jobTitle"))
            {
                Debug.WriteLine("NodeDymoLibrary Adding Print Job Title: " + (string)jobDetails.jobTitle);
                printParams.JobTitle = (string)jobDetails.jobTitle;
            }
            if (PropertyExists(jobDetails, "copies"))
            {
                Debug.WriteLine("NodeDymoLibrary Adding Print Copies: " + (string)jobDetails.copies);
                printParams.Copies = (int)jobDetails.copies;
            }
            // Set some settings for this printing 
            //

            IPrintJob printJob = printer.CreatePrintJob(printParams);
            Debug.WriteLine("NodeDymoLibrary Print Job Created");

            //
            // Lets loop over these labels
            IDictionary<string, ILabel> label = new Dictionary<string, ILabel>();
            object[] suppliedLabels = (object[])jobDetails.labels;// Cast JS array as Object (no keys) 
            foreach (IDictionary<string, object> thisLabel in suppliedLabels)
            {
                var i = label.Count.ToString();
                if (!thisLabel.ContainsKey("filename"))
                {
                    Debug.WriteLine("Dymo.cs No `labels`[x].`filename` parameter");
                    throw new System.ArgumentException("'labels'.'filename' parameter must be defined for each label", "original");
                }
//                if (!File.Exists((string)thisLabel["filename"]))
//                {
//                    Debug.WriteLine("Dymo.cs Unable to find label filename: " + (string)thisLabel["filename"]);
//                    throw new System.ArgumentException("'labels'.'filename' parameter must point to an existing file", "original");
//                }

                Debug.WriteLine("NodeDymoLibrary Adding label: " + (string)thisLabel["filename"]);
                try
                {
                    label[i] = Label.Open((string)thisLabel["filename"]);
                }
                catch( Exception ex)
                {
                    throw ex;
                }

                if (thisLabel.ContainsKey("fields"))
                {
                    Debug.WriteLine("NodeDymoLibrary Setting Field Values");
                    IDictionary<string, object> fields = (IDictionary<string, object>)thisLabel["fields"];
                    foreach (var kv in fields)
                    {
                        try
                        {
                            var k = kv.Key.ToUpper();
                            var obj = label[i].GetObjectByName(k);
                            if (obj != null)
                            {
                                var v = kv.Value.ToString();
                                label[i].SetObjectText(k, v);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                    }
                }

                if (thisLabel.ContainsKey("images"))
                {
                    Debug.WriteLine("NodeDymoLibrary Setting Image Values");
                    IDictionary<string, object> images = (IDictionary<string, object>)thisLabel["images"];
                    foreach (var kv in images)
                    {
                        try
                        {
                            var k = kv.Key.ToUpper();
                            var obj = label[i].GetObjectByName(k);
                            if (obj != null)
                            {
                                var v = kv.Value;
                                label[i].SetImagePngData(k, new MemoryStream((byte[])v));
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }


                Debug.WriteLine("NodeDymoLibrary Add Label to print job");
                printJob.AddLabel(label[i]);
            }


            Debug.WriteLine("NodeDymoLibrary Lets Print the Label/s");
            try
            {
                printJob.Print();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Debug.WriteLine("NodeDymoLibrary Label/s Printed");
            return true;
        }

        private static bool PropertyExists(dynamic source, string propertyName)
        {
            if (source is ExpandoObject)
                return ((IDictionary<string, object>)source).ContainsKey(propertyName);

            return source.GetType().GetProperty(propertyName) != null;
        }
    }
}
