using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMenager: MonoBehaviour
{
  [SerializeField] private string nomeCena;
  [SerializeField] private GameObject painelMenuInicial;
  [SerializeField] private GameObject painelOpçoes;
  public void Jogar()
  {
    SceneManager.LoadScene(nomeCena);
  }

  public void AbrirOpçoes()
  {
    painelMenuInicial.SetActive(false);
    painelOpçoes.SetActive(true);
  }

  public void FecharOpçoes()
  {
    painelOpçoes.SetActive(false);
    painelMenuInicial.SetActive(true);  
  }
  public void Sair()
  {
    Application.Quit();
  } 
}
