using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonCognito.Models
{
    public enum ToastIcon
    {
        none,
        warning,
        success,
        error,
        information 
    }
    public class ToastNotification
    {
        public string Text { get; set; }
        public string Heading { get; set; }
        public bool AutoHide { get; set; }
        public int HideAfter { get; set; } //Will be converted into milliseconds

        public ToastIcon Icon { get; set; }

        public ToastNotification()
        {
            HideAfter = 5;
            AutoHide = true;
        }
    }
}