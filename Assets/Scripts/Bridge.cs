using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Runtime.InteropServices;


public class Bridge : MonoBehaviour
{
    //this is the function we write in the jslib file in plugins folder
    // to access javascript function from unity c#
    //********
    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ShowMessage(string message);
    #endif
    //**********
    public TMP_InputField amount;
    public TMP_Text output; //once you receive the confirmation from razorpay
    //the payment details will be shown in this text


    [Serializable]
    public class payObject //object class for razorpay initialization
    {
        public string amount;
        public string currency;
        public string receipt;
    }
    public class responsePay //reponse we receive from razorpay fro checkout
    {
        public string id;
        public string entity;
        public int amount;
        public int amount_paid;
        public int amount_due;
        public string currency;
        public string receipt;
        public string offer_id;
        public string status;
        public int attempt;
        public string notes;
        public DateTime created_at;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnButtonClick()
    {
        try
        {
            payObject myObject = new payObject();
            myObject.amount = amount.text;
            myObject.currency = "INR";
            myObject.receipt = "receipt_11111"; //receipt id you have to enter manually as per your backend table
            //also this post method has to be done in backend server
            //right now in unity it works fine but when you build the project it wont accept the request
            string jsonData = JsonUtility.ToJson(myObject);
            Debug.Log(jsonData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            string authorization = authenticate("Key_id", "Key_secret"); //enter your key_id and key_secret
            string url = "https://api.razorpay.com/v1/orders";//api for razorpay order
            Debug.Log(amount.text);
            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("AUTHORIZATION", authorization);
            StartCoroutine(onResponse(request));
        }
        catch (Exception e) { Debug.Log("ERROR : " + e.Message); }
    }
    string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }
    IEnumerator onResponse(UnityWebRequest req)
    {
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
        }

        else
        {
            Debug.Log("Success " + req.downloadHandler.text);

            //folowing code is for sending the received data from razorpay to javascript function
            //***********
            #if UNITY_WEBGL && !UNITY_EDITOR
                ShowMessage(req.downloadHandler.text); 
            #endif

            //***************
            responsePay rp = JsonUtility.FromJson<responsePay>(req.downloadHandler.text);
            Debug.Log("1 : " + rp.id);
        }
    }

    // following function is called after checkout payment is done from javascript
    // //i.e, Index.html once the web build is created
    //this function will be called from javascript 
    public void SendToUnity(string message)
    {
        output.text = message;
    }

    //in the unity assets folder i have created folder called Plugins
    //inside that i have one more file called jscall.jslib
    //this file you have to create manually in your project to call javascript function

    //following code had to be added into jslib file

    //*****************
    //mergeInto(LibraryManager.library,{
    //ShowMessage : function(message) 
    //{
     //   window.acceptrazorpay(Pointer_stringify(message));
    //},
    //});

    //Once webgl build is done
    //edit index.html
    // add the following code inside body

    //**************************************
//    <script src = "https://checkout.razorpay.com/v1/checkout.js" ></ script >
//    < script >
//      function acceptrazorpay(msg)
//    {
//        console.log(msg);
//        var t = JSON.parse(msg);
//        var options = {
//          "key": "rzp_live_ne0gOTBWFqmXmP", // Enter the Key ID generated from the Dashboard
//          "amount": t.amount, // Amount is in currency subunits. Default currency is INR. Hence, 50000 refers to 50000 paise
//          "currency": t.currency,
//          "name": "Not Yet Decided",
//          "description": "Test Transaction",
//          "image": "https://example.com/your_logo",
//          "order_id": t.id, //This is a sample Order ID. Pass the `id` obtained in the response of Step 1
//          "handler": function(response){
//            // alert(response.razorpay_payment_id);
//            //alert(response.razorpay_order_id);
//            //alert(response.razorpay_signature);
//            unityInstance.SendMessage("Bridge", "SendToUnity", JSON.stringify(response));
//        },
//          "prefill": {
//            "name": "Prajwal",
//              "email": "prajwal@notyetdecided.com",
//              "contact": "000000"
//          },
//          "notes": {
//            "address": "Razorpay Corporate Office"
//          },
//          "theme": {
//            "color": "#3399cc"
//          }
//    };
//}
//window.acceptrazorpay = acceptrazorpay;
//    </ script >
//********************************
//that's it you are good to go
}
