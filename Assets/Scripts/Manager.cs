using UnityEngine;
using System.Collections.Generic;


public class Manager : MonoBehaviour
{
    public GameObject Onion;
    public GameObject Onion2;
    public GameObject Carrot;
    public GameObject Carrot2;
    public GameObject Rice;
    public GameObject Rice2;
    public GameObject Beef;
    public GameObject Beef2;
    public GameObject Item;
    public List<(Dictionary<GameObject, int> map, int minMoney)> runs = new List<(Dictionary<GameObject, int>, int)>();
    public int selectedRunIndex = 0;
    
    public void Start()
    {
        runs.Add((createMap(68500, 70000, 22500, 95000, 60000, 70000, 68000, 21000, 97000), 457000));
        runs.Add((createMap(70000, 72000, 23000, 100000, 62500, 71000, 69000, 22000, 98000), 467000));
        runs.Add((createMap(72000, 72500, 27000, 102500, 66000, 73000, 71000, 26000, 103500), 485500));
        runs.Add((createMap(75000, 75000, 30000, 105000, 71000, 75000, 76000, 29500, 106000), 509500));
        runs.Add((createMap(79500, 79500, 35000, 110500, 73000, 78000, 80500, 34000, 112000), 539000));

        int s = Random.Range(0, runs.Count);
        Dictionary<GameObject, int> selectedRun = runs[s].map;
        selectedRunIndex = s;
        ItemValue onionValue = Onion.GetComponent<ItemValue>();
        onionValue.cost = selectedRun[Onion];
        ItemValue itemValue = Item.GetComponent<ItemValue>();
        itemValue.cost = selectedRun[Item];
        ItemValue carrotValue = Carrot.GetComponent<ItemValue>();
        carrotValue.cost = selectedRun[Carrot];
        ItemValue riceValue = Rice.GetComponent<ItemValue>();
        riceValue.cost = selectedRun[Rice];
        ItemValue beefValue = Beef.GetComponent<ItemValue>();
        beefValue.cost = selectedRun[Beef];
        ItemValue onion2Value = Onion2.GetComponent<ItemValue>();
        onion2Value.cost = selectedRun[Onion2];
        ItemValue carrot2Value = Carrot2.GetComponent<ItemValue>();
        carrot2Value.cost = selectedRun[Carrot2];
        ItemValue rice2Value = Rice2.GetComponent<ItemValue>();
        rice2Value.cost = selectedRun[Rice2];
        ItemValue beef2Value = Beef2.GetComponent<ItemValue>();
        beef2Value.cost = selectedRun[Beef2];

    }

    Dictionary<GameObject,int> createMap(int onionPrice, int carrotPrice, int ricePrice, int beefPrice, int itemPrice, int otherOnionPrice, int otherCarrotPrice, int otherRicePrice, int otherBeefPrice)
    {
        Dictionary<GameObject,int> map = new Dictionary<GameObject, int>
        {
            { Onion, onionPrice },
            { Carrot, carrotPrice },
            { Rice, ricePrice },
            { Beef, beefPrice },
            { Item, itemPrice },
            { Onion2, otherOnionPrice },
            { Carrot2, otherCarrotPrice },
            { Rice2, otherRicePrice },
            { Beef2, otherBeefPrice }
        };
        return map;
    }
    

}
