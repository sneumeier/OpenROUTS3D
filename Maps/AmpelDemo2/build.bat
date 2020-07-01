python "C:\Program Files (x86)\DLR\Sumo\tools\randomTrips.py"^
 -n AmpelDemo2.net.xml --seed 100 --fringe-factor 10 -p 4^
 -r AmpelDemo2.rou.xml -o AmpelDemo2.trips.xml^
 -e 18000^
 --vehicle-class passenger^
 --vclass passenger^
 --prefix veh^
 --min-distance 3^
 --trip-attributes speedDev=\"0.1\" departLane=\"best\"^
 --validate
