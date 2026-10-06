using UnityEngine;
using System.Collections.Generic;
public class DroneManager : MonoBehaviour
{
    public GameObject dronePrefab;
    public int maxDrones = 3;
    public float baseOrbitRadius = 2.2f, radiusSpacing = 0.3f;
    public float fireBonus, rangeBonus, speedBonus;
    public int extraBullets;
    readonly List<DroneController> drones = new List<DroneController>();
    public void AddDrone()
    {
        if (drones.Count >= maxDrones) return;
        var drone = Instantiate(dronePrefab, transform.position, Quaternion.identity).GetComponent<DroneController>();
        drones.Add(drone); Sync();
    }
    public void Sync()
    {
        for (int i = 0; i < drones.Count; i++)
        {
            var d = drones[i]; d.baseAngle = i * 360f / drones.Count;
            d.orbitRadius = baseOrbitRadius + radiusSpacing * drones.Count;
            d.fireRateBonus = fireBonus; d.rangeBonus = rangeBonus; d.bulletSpeedBonus = speedBonus; d.extraBullets = extraBullets;
        }
    }
    public List<DroneController> GetDrones() => drones;
}
