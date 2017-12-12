using UnityEngine;
using System.Collections.Generic;

public class PhoneContact
{
	public string Number ;
	public string Type ;
}

public class EmailContact
{
	public string Address;
	public string Type ;
}

//holder of person details
public class Contact
{
	public string Id ;
	public string Name ;
	
	public List<PhoneContact> Phones = new List<PhoneContact>();
	public List<EmailContact> Emails = new List<EmailContact>();
	
	public List<string> Connections = new List<string>();//for android only. example(google,whatsup,...)

	public Texture2D PhotoTexture ;
	
	public void FromBytes( byte[] bytes )
	{
	
		System.IO.BinaryReader reader = new System.IO.BinaryReader( new System.IO.MemoryStream( bytes ));
		
		Id = ReadString( reader );
		Name = ReadString( reader );
		
		short size = reader.ReadInt16();
		Log( "Photo size == " + size );
		if( size > 0 )
		{
			byte[] photo = reader.ReadBytes( (int)size);
			PhotoTexture = new Texture2D(2,2);
			PhotoTexture.LoadImage( photo );
		}
		
		size = reader.ReadInt16();
		Log( "Phones size == " + size );
		if( size > 0 )
		{
			for( int i = 0 ; i < size ; i++ )
			{
				PhoneContact pc = new PhoneContact();
				pc.Number = ReadString( reader );
				pc.Type = ReadString( reader );
				Phones.Add( pc );
			}
		}
		
		size = reader.ReadInt16();
		Log( "Emails size == " + size );
		if( size > 0 )
		{
			for( int i = 0 ; i < size ; i++ )
			{
				EmailContact ec = new EmailContact();
				ec.Address = ReadString( reader );
				ec.Type = ReadString( reader );
				Emails.Add( ec );
			}
		}
		
		size = reader.ReadInt16();
		Log( "Connections size == " + size );
		if( size > 0 )
		{
			for( int i = 0 ; i < size ; i++ )
			{
				string connection = ReadString( reader );
				Connections.Add( connection );
			}
		}
	}
	
	string ReadString( System.IO.BinaryReader reader )
	{
		string res = "";
		short size = reader.ReadInt16();
		Log( "read string of size " + size );
		if( size == 0 )
			return res;
		
		byte[] data = reader.ReadBytes( size);
		res = System.Text.Encoding.UTF8.GetString( data );

		Log( "string " + res + " is " + res);
	
		return res;
		
	}

	void Log( string message )
	{
		//Debug.Log( message );
	}
}
