using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using NUnit.Framework;
public class BagTrigger : MonoBehaviour
{
    // Tag to detect (optional)
    [SerializeField] private string detectableTag = "Food";
    public AudioSource audioSource; // assign in inspector
    public AudioClip clip;          // assign the clip you want to play
    public AudioClip wrongClip;
    public int totalMoney = 547500;
    public TextMeshProUGUI moneyText;
    private Dictionary<String,(int,int)> inventory = new Dictionary<String,(int,int)>();
    public TextMeshProUGUI inventoryText;
    public GameObject inventoryUIOff;
    private Manager manager;
    private int minMoneyThisRun = 0;


    private void Start()
    {
        inventoryUIOff.SetActive(false);
        totalMoney = 547500;
        UpdateMoneyUI();
        manager = FindFirstObjectByType<Manager>();
        if (manager != null)
        {
            // Assuming Manager has already picked a random run in Start()
            this.minMoneyThisRun = manager.runs[manager.selectedRunIndex].minMoney;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // If you want to detect ALL objects:
        // Debug.Log(other.name + " entered the bag!");

        // If you want to detect ONLY onions or items with a tag
        if (other.CompareTag(detectableTag))
        {
            Debug.Log("A detectable object entered the bag: " + other.name);
            HandleObjectEntered(other.gameObject);
        }
    }

    private void HandleObjectEntered(GameObject obj)
    {//bag logic
        ItemValue value = obj.GetComponentInParent<ItemValue>();
        if (audioSource != null && clip != null && totalMoney - value.cost >=0)
        {
            totalMoney -= value.cost;
            audioSource.PlayOneShot(clip);
            UpdateMoneyUI();
            string parentTag = obj.transform.parent.tag;
            if(inventory.ContainsKey(parentTag))
            {
                var old = inventory[parentTag];
                inventory[parentTag] = (old.Item1 + 1, old.Item2);
            }
            else
            {
                inventory.Add(parentTag, (1,value.cost));
            }
            obj.SetActive(false);
        }
        else
        {
            audioSource.PlayOneShot(wrongClip);
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = totalMoney + "Som";
        }
    }

    public void showUI()
    {
        this.inventoryText.text = "Inventarizatsiya:\n";
        foreach (var item in inventory)
        {
            this.inventoryText.text += item.Key +": Miqdor = "+item.Value.Item1+", Narx = "+item.Value.Item2+"Som\n";
        }
        int spent = 547500-totalMoney;
        this.inventoryText.text += "Sarflagan Jami Pul: " + spent + "Som\n";
        this.inventoryText.text += "Kamida kerakli summa: " + minMoneyThisRun + "Som";
        inventoryUIOff.SetActive(true);
    }
}
