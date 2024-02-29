namespace GameRoot;


public interface IInteractor
{
    public void Interact(Interactable3D interactable);
    public void CancelInteract(Interactable3D interactable);
    public void Focus(Interactable3D interactable);
    public void UnFocus(Interactable3D interactable);

}