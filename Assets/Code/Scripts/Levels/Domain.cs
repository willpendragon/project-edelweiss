using UnityEngine;

[CreateAssetMenu(fileName = "Domain", menuName = "Level Design/Domain", order = 1)]
public class Domain : ScriptableObject
{
    public Level[] levelList;
    public string domainName;
    public int nextDomainRequirement;
    public int bossFightRequirement;
    public int clearRequirement;
}
