public class ShopItemData
{
    private readonly int _id;
    private readonly float _price;
    private readonly bool _isUnlocked;
    private readonly bool _canBuy;

    public ShopItemData(int id, float price, bool isUnlocked)
    {
        _id = id;
        _price = price;
        _isUnlocked = isUnlocked;
    }

    public int Id => _id;
    public float Price => _price;
    public bool IsUnlocked => _isUnlocked;
}
