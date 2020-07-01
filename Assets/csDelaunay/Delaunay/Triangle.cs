using System.Collections;
using System.Collections.Generic;

namespace csDelaunay {

	public class Triangle {

		private List<Site> sites;
		public List<Site> Sites {get{return sites;}}
        public Site sa;
        public Site sb;
        public Site sc;

		public Triangle(Site a, Site b, Site c) {
			sites = new List<Site>();
			sites.Add(a);
			sites.Add(b);
			sites.Add(c);
            sa = a;
            sb = b;
            sc = c;
		}

		public void Dispose() {
			sites.Clear();
		}
	}
}