using UnityEngine;

public class CustomerClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        CustomerManager.Instance.ServeCurrentCustomer();
    }
}
