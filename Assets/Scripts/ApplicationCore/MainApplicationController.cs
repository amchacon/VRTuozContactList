using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;

public class MainApplicationController : MonoBehaviour
{
    [SerializeField] Transform contactsList;
    [SerializeField] Transform selectedContactsPanel;
    [SerializeField] GameObject contactsPage;
    [SerializeField] GameObject contactLinePrefab;
    [SerializeField] GameObject selectedContactPrefab;

    [SerializeField] int maxContactsToShow;
    [SerializeField] Button previousPageButton;
    [SerializeField] Button nextPageButton;

    [HideInInspector] public List<GameObject> contactsObjects;
    [HideInInspector] public List<ContactLine> selectedContacts;
    #if UNITY_EDITOR
    public List<ContactInfo> contactsInfoList;
    #endif
    private int contactsCount;
    private int pageCount;

    void Start()
    {
        #if !UNITY_EDITOR
        Contacts.LoadContactList(onDone, onLoadFailed);
        #endif
        StartCoroutine(SetupContactList());
    }

    //SessionCheck

    IEnumerator SetupContactList()
    {
#if UNITY_EDITOR
        yield return new WaitForSeconds(.1f);
        contactsCount = contactsInfoList.Count;
#endif
#if !UNITY_EDITOR
        yield return new WaitWhile(() => contactsLoaded == false);
        contactsCount = Contacts.ContactsList.Count;
#endif

        pageCount = (int)System.Math.Ceiling((decimal)contactsCount / (decimal)maxContactsToShow);
        for (int p = 0; p < pageCount; p++)
        {
            GameObject page = (GameObject)Instantiate(contactsPage, contactsList);
            page.name = "Page " + p;
            int lastIndex = contactsObjects.Count;
            for (int i = lastIndex; i < contactsCount; i++)
            {
                ContactLine contactLine = (ContactLine)Instantiate(contactLinePrefab, page.transform).GetComponent<ContactLine>();

                #if UNITY_EDITOR
                contactLine.name = contactsInfoList[i].user;
                contactLine.info.id = contactsInfoList[i].id;
                contactLine.info.photo = contactsInfoList[i].photo;
                contactLine.info.user = contactsInfoList[i].user;
                contactLine.info.number = contactsInfoList[i].number;
                contactLine.info.email = contactsInfoList[i].email;
                #endif

                #if !UNITY_EDITOR
                Contact c = Contacts.ContactsList[i];
                contactLine.name = c.Name;
                contactLine.info.id = c.Id;
                contactLine.info.photo = c.PhotoTexture;
                contactLine.info.user = c.Name;
                contactLine.info.number = c.Phones[0].Number;
                contactLine.info.email = c.Emails[0].Address;
                #endif

                contactLine.Initialize();
                contactsObjects.Add(contactLine.gameObject);
                contactLine.GetComponent<Button>().onClick.AddListener(delegate () { SelectContact(contactLine); });
                if (page.transform.childCount >= maxContactsToShow)
                    break;
            }
            page.gameObject.SetActive(p < 1);
        }
        SetupPaginationButtons();
    }

#region Pagination
    private void SetupPaginationButtons()
    {
        if (previousPageButton)
        {
            if (contactsObjects.Count <= maxContactsToShow)
                return;
            previousPageButton.onClick.AddListener(delegate () { PreviousPage(); });
        }

        if (nextPageButton)
        {
            if (contactsObjects.Count <= maxContactsToShow)
                return;
            nextPageButton.onClick.AddListener(delegate () { NextPage(); });
        }
    }

    public void PreviousPage()
    {
        contactsList.GetChild(0).gameObject.SetActive(false);
        contactsList.GetChild(pageCount-1).transform.SetAsFirstSibling();
        contactsList.GetChild(0).gameObject.SetActive(true);
    }

    public void NextPage()
    {
        contactsList.GetChild(0).gameObject.SetActive(false);
        contactsList.GetChild(0).transform.SetAsLastSibling();
        contactsList.GetChild(0).gameObject.SetActive(true);
    }
#endregion

    //SelectedContactsList
    private void SelectContact(ContactLine contact)
    {
        if(!selectedContacts.Exists(x => x.info.id == contact.info.id))
        {
            SelectedContact info = (SelectedContact)Instantiate(selectedContactPrefab, selectedContactsPanel).GetComponent<SelectedContact>();
            info.id = contact.info.id;
            info.photo.texture = contact.info.photo;
            info.userText.text = contact.info.user;
            selectedContacts.Add(contact);
        }
        else
        {
            selectedContacts.Remove(contact);
            Destroy(selectedContactsPanel.GetComponentsInChildren<SelectedContact>().FirstOrDefault(c => c.id == contact.info.id).gameObject);
        }
    }

    public void ValidateButton()
    {
        foreach (var item in selectedContacts)
        {
            Debug.Log("Name: " + item.info.user + " | Number: " + item.info.number + " | E-mail: " + item.info.email);
        }
    }

#if !UNITY_EDITOR
    bool contactsLoaded = false;
    string failString;
    void onLoadFailed(string reason)
    {
        failString = reason;
    }

    void onDone()
    {
        failString = null;
        contactsLoaded = true;
    }
#endif
}
