namespace FruitHub.ApplicationCore.Interfaces;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}