using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public class Contacts{

	//looks likee these four fields down doesnot working for all mobiles
	public static String 		MyPhoneNumber ;
	public static String 		SimSerialNumber ;
	public static String 		NetworkOperator ;
	public static String 		NetworkCountryIso ;
	//
	public static List<Contact> ContactsList = new List<Contact>();

	#if UNITY_ANDROID
	static AndroidJavaObject activity;
	static AndroidJavaClass ojc = null ;
	#endif

	static System.Action<string> onFailed;
	static System.Action onDone;
	public static void LoadContactList( )
	{
		LoadContactList( null , null);
	}

	public static void LoadContactList( System.Action _onDone, System.Action<string> _onFailed )
	{
        UnityEngine.Debug.Log ( "LoadContactList at " + Time.realtimeSinceStartup );
		onFailed = _onFailed;
		onDone = _onDone;

		GameObject helper = new GameObject ();
		GameObject.DontDestroyOnLoad( helper);
		helper.name = "ContactsListMessageReceiver";
		helper.AddComponent<MssageReceiver> ();

		#if UNITY_ANDROID
		ojc = new AndroidJavaClass("com.vrtuoz.unitynativeplugin.ContactList");
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
		ojc.CallStatic("LoadInformation" , activity , true, true,true,true);
		#endif
	}

	public static void GetContact( int index )
	{
		byte[] data = null;

		#if UNITY_ANDROID
		data = ojc.CallStatic<byte[]>("getContact" , index);
		#endif
		Contact c 	= new Contact();
		Debug( "Data length for " + index + " is " + data.Length );
		c.FromBytes( data );
		Contacts.ContactsList.Add( c );
	}

	public static void OnInitializeDone()
	{
        UnityEngine.Debug.Log ( "done at " + Time.realtimeSinceStartup );
		if (onDone != null) 
		{
			onDone();
		}
	}

	public static void OnInitializeFail( string message )
	{
        UnityEngine.Debug.Log ( "fail at " + Time.realtimeSinceStartup );
		if (onFailed != null) 
		{
			onFailed( message );
		}
	}

	static void Debug( string message)
	{
		//Debug.Log ( message );
	}

}
