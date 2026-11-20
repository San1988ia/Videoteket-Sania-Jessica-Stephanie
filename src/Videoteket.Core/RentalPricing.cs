static class RentalPricing
{
    public static int GetPricePerDay(bool isNew)
    {
        return isNew ? 49 : 29;
    }
}
