using UnityEngine;
/// <summary>
/// Handles mouse click events on a customer to serve them.
/// </summary>
public class CustomerClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        CustomerManager.Instance.ServeCurrentCustomer();
    }
}
