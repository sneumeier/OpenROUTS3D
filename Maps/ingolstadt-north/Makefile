MAP := ingolstadt-north
REMOVE_EDGE_TYPES := highway.raceway,highway.bridleway,highway.path,highway.track,highway.service,highway.footway,highway.cycleway,highway.steps,railway.rail
TYPEMAP_NETCONVERT := osmNetconvert.typ.xml
TYPEMAP_POLYCONVERT := osmPolyconvert.typ.xml

$(MAP).osm: oberbayern-20160115.osm.bz2
	bzcat $< | osmosis --read-xml enableDateParsing=no file=- --bounding-box top=48.7810 left=11.4305 bottom=48.7646 right=11.4738 --write-xml file=$@

#$(MAP).nod.xml $(MAP).edg.xml $(MAP).con.xml: $(MAP).osm | $(TYPEMAP_NETCONVERT)
	#netconvert --osm-files $< --plain-output-prefix $(MAP) -t $(TYPEMAP_NETCONVERT) --proj.utm --geometry.remove --roundabouts.guess --ramps.guess --tls.guess-signals --tls.discard-simple --tls.join --no-turnarounds

#$(MAP).net.xml: $(MAP).nod.xml $(MAP).edg.xml $(MAP).con.xml | $(TYPEMAP_NETCONVERT)
	#netconvert -t $(TYPEMAP_NETCONVERT) -n $(MAP).nod.xml -e $(MAP).edg.xml -x $(MAP).con.xml --no-turnarounds --junctions.join --remove-edges.isolated -o $@

$(MAP).net.xml: $(MAP).osm | $(TYPEMAP_NETCONVERT)
	netconvert --osm-files $< -t $(TYPEMAP_NETCONVERT) -o $@ --proj.utm --geometry.remove --roundabouts.guess --ramps.guess --tls.guess-signals --tls.discard-simple --tls.join --no-turnarounds --junctions.join --remove-edges.isolated --remove-edges.by-type $(REMOVE_EDGE_TYPES)

$(MAP).poly.xml: $(MAP).osm $(MAP).net.xml | $(TYPEMAP_POLYCONVERT)
	polyconvert --osm-files $< --osm.keep-full-type --net-file $(MAP).net.xml --type-file $(TYPEMAP_POLYCONVERT) --prune.in-net --proj.utm --output-file $@

$(MAP)-netedit.rou.xml: $(MAP)-netedit.net.xml
	randomTrips.py -n $< -o $(MAP).trips.xml -r $@ -e 1800 -L

run: $(MAP).sumocfg $(MAP)-netedit.net.xml $(MAP)-netedit.rou.xml $(MAP).poly.xml
	sumo-gui -c $<

zip: $(MAP).sumocfg $(MAP).net.xml $(MAP).rou.xml $(MAP).rou.a9.xml $(MAP).poly.xml
	zip $(MAP).zip $^

clean:
	rm -f $(MAP).nod.xml $(MAP).edg.xml $(MAP).con.xml $(MAP).net.xml $(MAP).poly.xml $(MAP).rou.xml
