#include "sdk.h"

using namespace std;

bool SPECTRUM::intesityGreater(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2) {
	return stPeak1.lfIntens > stPeak2.lfIntens;
}

bool SPECTRUM::intesityLesser(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2) {
	return stPeak1.lfIntens < stPeak2.lfIntens;
}

bool SPECTRUM::mzGreater(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2) {
	return stPeak1.lfMz > stPeak2.lfMz;
}

bool SPECTRUM::mzLesser(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2) {
	return stPeak1.lfMz < stPeak2.lfMz;
}

std::ostream &operator<<(ostream &stOut, const SPECTRUM &stSpec) {
	stOut << fixed << setprecision(FIXED_PRECISION);
	stOut << stSpec.strSpecTitle << " " << stSpec.nPrecurNum << " "
		  << stSpec.nPeaksNum;
	return stOut;
}

std::fstream &operator<<(fstream &stOut, const SPECTRUM &stSpec) {
	stOut << fixed << setprecision(FIXED_PRECISION);
	stOut << stSpec.strSpecTitle << " " << stSpec.nPrecurNum << " "
		<< stSpec.nPeaksNum;
	return stOut;
}

std::istream &operator>>(std::istream &stIn, SPECTRUM &stSpec) {
	stIn >> stSpec.strSpecTitle >> stSpec.nPrecurNum >> stSpec.nPeaksNum;
	return stIn;
}

std::fstream &operator>>(std::fstream &stIn, SPECTRUM &stSpec) {
	stIn >> stSpec.strSpecTitle >> stSpec.nPrecurNum >> stSpec.nPeaksNum;
	return stIn;
}

void SPECTRUM::copyBasicItems(const SPECTRUM &stSpec) {
	strSpecTitle = stSpec.strSpecTitle;
	m_nScanNo = stSpec.m_nScanNo;
	nPrecurNum = stSpec.nPrecurNum;
	nPeaksNum = stSpec.nPeaksNum;
	
}

