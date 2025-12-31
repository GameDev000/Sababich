// using UnityEngine;
// /// <summary>
// /// Handles mouse click events on a customer to serve them.
// /// </summary>
// public class CustomerClick : MonoBehaviour
// {
//     private void OnMouseDown()
//     {
//         CustomerManager.Instance.ServeCurrentCustomer();
//     }
// }

using UnityEngine;

/// <summary>
/// Click on a customer -> serve THAT customer.
/// </summary>
public class CustomerClick : MonoBehaviour
{
    private Customer customer;

    private void Awake()
    {
        customer = GetComponent<Customer>();
    }

    private void OnMouseDown()
    {
        if (customer == null) return;
        if (CustomerManager.Instance == null) return;

        CustomerManager.Instance.ServeCustomer(customer);
    }
}
