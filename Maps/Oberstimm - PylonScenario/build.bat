python "C:\Program Files (x86)\DLR\Sumo\tools\randomTrips.py"^
 -n oberstimm.net.xml --seed 42 --fringe-factor 5 -p 12.141345^
 -r oberstimm.passenger.rou.xml -o oberstimm.passenger.trips.xml^
 -e 3600^
 --vehicle-class passenger^
 --vclass passenger^
 --prefix veh^
 --min-distance 300^
 --trip-attributes speedDev=\"0.1\" departLane=\"best\"^
 --validate
