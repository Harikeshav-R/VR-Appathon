using UnityEngine;

public class ItemValue : MonoBehaviour
{
    public int cost;

    [System.Serializable]
    public struct PriceAudio
    {
        public int price;
        public AudioClip clip;
    }

    public PriceAudio[] priceAudioList;

    public AudioClip GetAudioForPrice(int currentPrice)
    {
        foreach (var item in priceAudioList)
        {
            if (item.price == currentPrice)
                return item.clip;
        }
        return null;
    }
}
