using UnityEngine;

public class Modifier : MonoBehaviour
{
    [SerializeField]
    private int _value;
    public int Value { get => _value; set => _value = value; }     
}
