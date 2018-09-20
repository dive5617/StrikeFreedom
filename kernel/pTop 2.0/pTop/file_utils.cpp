#include "file_utils.h"
#include "util.h"

using namespace std;

// created by wrm @2016.11.20

MS2InputFactory::MS2InputFactory()
{
}

MS2InputFactory::~MS2InputFactory()
{
}


MS2Input *MS2InputFactory::getImporter(MS2FormatType eType)
{
	switch (eType) {
	case(MFT_MGF) :
		return new MGFInput;
	case(MFT_PF) :
		return new PFInput;
	//case(MFT_MS2) :
	//	return new MS2TypeInput;
		//	case(MFT_RAW):
		//		return new RAWInput;
		//	case(MFT_DTA):
		//		return new DTAInput;
		//	case(MFT_DTAS):
		//		return new DTASInput;
		//	case(MFT_PKL):
		//		return new PKLInput;
		//	case(MFT_SDTA):
		//		return new DTASingleInput;
	default:
		CErrInfo err("MS2InputFactory", "getImporter", "unkown input type.");
		throw runtime_error(err.Get());
		return NULL;
	}
}

/*begin: class MGFInput*/
// Changed strcut SPECTRUM，每张谱图对应一个母离子列表 
// 这个函数兼容其他的mgf文件，即其实每张谱图的母离子列表中只有一个元素
// 如果处理的是pParseTD导出的MGF文件，则scan号的数目通过解析title获得，否则scan数目等于谱图数目
MGFInput::MGFInput() :
m_cSeperator(' '), m_tTotalScan(0), m_tTotalSpec(0), m_tCurSpec(0), m_tCurLine(0)
{
}

MGFInput::~MGFInput() {
	closeInFile();
}

void MGFInput::loadAll(const string &strPath, vector<SPECTRUM> &vSpec)
{
	try {
		startLoad(strPath.c_str());

		vSpec.clear();
		SPECTRUM spec;
		int idx = 0;

		for (int i = 0; i < m_tTotalSpec; ++i) {
			loadNext(spec, idx);
			vSpec.push_back(spec);
		}
		endLoad();
	}
	catch (exception &e) {
		CErrInfo info("MGFInput", "loadAll", "StartLoad failed.");
		throw runtime_error(info.Get(e));
	}
	catch (...) {
		CErrInfo info("MGFInput", "loadAll", "caught an unknown exception from StartLoad().");
		throw runtime_error(info.Get());
	}
}

void MGFInput::openInFile(const string &strFilePath)
{
	m_stIn.open(strFilePath.c_str());
	if (m_stIn.fail()) {
		CErrInfo info("MGFInput", "openInFile", "m_ifIn.fail() when open file: " + strFilePath + " failed.");
		throw runtime_error(info.Get());
	}
	m_strFilePath = strFilePath;
}

void MGFInput::closeInFile()
{
	if (m_stIn.is_open())
		m_stIn.close();
	m_stIn.clear();  // 将流的状态复位
}

// first time to traverse the file and get the number of spectra
size_t MGFInput::checkLegalityAndCount()
{
	size_t tSpecNum = 0;
	unordered_set<int> scans;
	try {
		string strLine;

		size_t tLineNum = 0;
		//m_vSpecLineNum.clear();
		//m_vSpecLineNum.reserve(10000);
		m_vPeakNum.clear();
		m_vPeakNum.reserve(10000);  // 
		bool bTitle(false), bCharge(false), bPepMass(false);
		while (getline(m_stIn, strLine)) {
			CStringProcess::Trim(strLine);


			if (!strLine.compare("BEGIN IONS")) {
				//m_vSpecLineNum.push_back(tLineNum);
				m_vPeakNum.push_back(0);
				bTitle = bCharge = bPepMass = false;
			}
			else if (!strLine.compare("END IONS")) {
				if (!bTitle || !bCharge || !bPepMass) {
					CErrInfo err("MGFInput", "checkLegalityAndCount", "loss information of title, charge or peptide m/z.");
					throw runtime_error(err.Get());
				}
				++tSpecNum;
			}
			else if (!strLine.substr(0, 5).compare("TITLE")) {
				bTitle = true;
				size_t tPos = strLine.find("=");
				if (tPos == string::npos) {
					ostringstream oss;
					oss << "error in MGF, line " << m_tCurLine;
					throw oss.str();
				}
				string strTitle = strLine.substr(tPos + 1, (int)strLine.size() - tPos - 1);
				CStringProcess::Trim(strTitle);
				vector<string> vec;
				CStringProcess::Split(strTitle, ".", vec);
				if (vec.size() >= 5){
					int tScanNo = atoi(vec[(int)vec.size()-5].c_str());
					scans.insert(tScanNo);
				}
			}
			else if (!strLine.substr(0, 6).compare("CHARGE")) {
				bCharge = true;
			}
			else if (!strLine.substr(0, 7).compare("PEPMASS")) {
				bPepMass = true;
			}
			else if (strLine.size() > 1 && isdigit(strLine[0])) {
				if (strLine.find(' ') != string::npos) {
					m_cSeperator = ' ';
				}
				else if (strLine.find('\t') != string::npos) {
					m_cSeperator = '\t';
				}
				else {
					ostringstream oss;
					oss << "error in MGF, line " << tLineNum + 1;
					throw oss.str();
				}
				vector<string> vec;
				CStringProcess::Split(strLine, string(1,m_cSeperator), vec);
				if (vec.size() < 2) {
					ostringstream oss;
					oss << "error in MGF, line " << tLineNum + 1;
					throw oss.str();
				}
				++ m_vPeakNum[tSpecNum];
			}
			else {
				// other not important information, just jump after
			}
			++tLineNum;
		}
	}
	catch (exception & e) {
		CErrInfo info("MGFInput", "checkLegalityAndCount", "there are problems in the MGF file.", e);
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("MGFInput", "checkLegalityAndCount", "caught an unknown exception.");
		throw runtime_error(info.Get().c_str());
	}
	m_tTotalScan = scans.size();
	return tSpecNum;
}

// open the file and count spectra number
void MGFInput::startLoad(const string &strPath)
{
	try {
		openInFile(strPath);
		m_tTotalSpec = checkLegalityAndCount();
		closeInFile();

		openInFile(strPath);
		m_tCurSpec = 0, m_tCurLine = 0;
	}
	catch (exception & e) {
		CErrInfo info("MGFInput", "startLoad", "OpenInFile: " + strPath + " failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("MGFInput", "startLoad", "caught an unknown exception from OpenInFile: " + strPath + " failed.");
		throw runtime_error(info.Get().c_str());
	}
	return;
}

void MGFInput::loadNextBatch(int nNumber, vector<SPECTRUM> &vSpec, int &nIdx)
{
	vSpec.clear();

	for (int i = 0; i < nNumber; ++i) {
		SPECTRUM spec;
		loadNext(spec, nIdx);
		vSpec.push_back(spec);
	}
}

void MGFInput::loadNext(SPECTRUM &spec, int &nIdx)
{
	string strLine;
	PRECURSOR precur;
	FRAGMENT_ION peak;
	while (getline(m_stIn, strLine)) {
		CStringProcess::Trim(strLine);
		if (!strLine.compare("BEGIN IONS")) {
			// begin to read
			spec.nPrecurNum = 1;
			spec.nCandidateProNum = 0;
			spec.nPeaksNum = m_vPeakNum[nIdx];
			spec.strSpecTitle = "NULL";
			spec.vPeaksTbl.reserve(m_vPeakNum[nIdx]);
			spec.vPrecursors.clear();
			precur.nValidCandidateProNum = 0;
			precur.lfPrecursorMass = 0.0;
			precur.nPrecursorCharge = 2;
			precur.aBestPrSMs.clear();
		}
		else if (!strLine.compare("END IONS")) {
			
			precur.lfPrecursorMass = (precur.lfPrecursorMass - IonMass_Proton) * (double)precur.nPrecursorCharge + IonMass_Proton;
			spec.vPrecursors.push_back(precur);
			++nIdx;
			++m_tCurLine;
			return;
		}
		else if (!strLine.substr(0, 5).compare("TITLE")) {
			size_t tPos = strLine.find("=");
			if (tPos == string::npos) {
				ostringstream oss;
				oss << "error in MGF, line " << m_tCurLine;
				throw oss.str();
			}
			spec.strSpecTitle = strLine.substr(tPos + 1, strLine.size() - tPos - 1);
			CStringProcess::Trim(spec.strSpecTitle);
			vector<string> vec;
			CStringProcess::Split(spec.strSpecTitle, ".", vec);
			if (vec.size() >= 5)
				spec.m_nScanNo = atoi(vec[(int)vec.size() - 4].c_str());
			else
				spec.m_nScanNo = nIdx;
		}
		else if (!strLine.substr(0, 6).compare("CHARGE")) {
			size_t tPos = strLine.find("=");
			if (tPos == string::npos) {
				ostringstream oss;
				oss << "error in MGF, line " << m_tCurLine;
				throw oss.str();
			}
			size_t tEnd = strLine.find("+", tPos);
			string strCharge = strLine.substr(tPos + 1, tEnd - tPos - 1);
			precur.nPrecursorCharge = atoi(strCharge.c_str());
		}
		else if (!strLine.substr(0, 7).compare("PEPMASS")) {
			size_t tPos = strLine.find("=");
			if (tPos == string::npos) {
				ostringstream oss;
				oss << "error in MGF, line " << m_tCurLine;
				throw oss.str();
			}
			string strMass = strLine.substr(tPos + 1, strLine.size() - tPos - 1);
			precur.lfPrecursorMass = atof(strMass.c_str());
		}
		else if (strLine.size() > 1 && isdigit(strLine[0])) {
			vector<string> vec;
			CStringProcess::Split(strLine, string(1,m_cSeperator), vec);
			CStringProcess::Trim(vec[0]);
			CStringProcess::Trim(vec[1]);
			peak.lfMz = atof(vec[0].c_str());
			peak.lfIntens = atof(vec[1].c_str());
			spec.vPeaksTbl.push_back(peak);
		}
		else {
			// other not important information, just jump after
		}
		++m_tCurLine;
	}
}

void MGFInput::loadPrev(SPECTRUM &spec, int &idx)
{
	// obsolete, no body will read mgf like this
}
void MGFInput::loadSpec(SPECTRUM &spec, const string &strTitle, int& nSpecNo)
{
	// obsolete
}

void MGFInput::endLoad()
{
	closeInFile();
}

int MGFInput::GetScanNum() const
{
	return m_tTotalScan;
}
int MGFInput::GetSpecNum() const
{
	return m_tTotalSpec;
}

/* end: class MGFInput */

/*begin: class PFInput*/
// 解析pF文件
// pF二进制文件的格式：谱图数目、谱图文件名长度、谱图文件名
// [scan号、谱峰数目、谱峰列表、母离子数目、母离子列表]
PFInput::PFInput() :
m_fp(NULL), m_tTotalScan(0), m_tTotalSpec(0)
{
}
PFInput::~PFInput()
{
	endLoad();
}

void PFInput::loadAll(const std::string &strPath, std::vector<SPECTRUM> &vSpec)
{
	try {
		startLoad(strPath.c_str());

		vSpec.clear();
		SPECTRUM spec;
		int idx = 0;

		for (int i = 0; i < m_tTotalScan; ++i) {
			loadNext(spec, idx);
			vSpec.push_back(spec);
		}
		endLoad();
	}
	catch (exception &e) {
		CErrInfo info("MGFInput", "loadAll", "StartLoad failed.");
		throw runtime_error(info.Get(e));
	}
	catch (...) {
		CErrInfo info("MGFInput", "loadAll", "caught an unknown exception from StartLoad().");
		throw runtime_error(info.Get());
	}
}

void PFInput::startLoad(const std::string &strPath)
{
	try {
		if (m_fp == NULL)
			m_fp = fopen(strPath.c_str(), "rb");
		if (m_fp == NULL){
			throw runtime_error(string("[Error] Failed to open file: ") + strPath);
		}
		m_tTotalSpec = 0;
		fread(&m_tTotalScan, sizeof(int), 1, m_fp);  // number of scans
		int nSize = 0;
		fread(&nSize, sizeof(int), 1, m_fp);
		char *szTitle = new char[nSize + 1];
		fread(szTitle, sizeof(char), nSize, m_fp);
		szTitle[nSize] = '\0';
		delete[] szTitle;

		for (int nIdx = 0; nIdx < m_tTotalScan; ++nIdx) {
			int nScan = 0;
			fread(&nScan, sizeof(int), 1, m_fp);  // scan 

			int nPeakNum = 0;
			fread(&nPeakNum, sizeof(int), 1, m_fp);
			double *lfPeaks = new double[nPeakNum << 1];
			fread(lfPeaks, sizeof(double), nPeakNum << 1, m_fp);

			int nPreNum = 0;
			fread(&nPreNum, sizeof(int), 1, m_fp);
			m_tTotalSpec += nPreNum;
			for (int j = 0; j < nPreNum; j++) {
				double lfMz = 0;
				int nCharge = 0;
				fread(&lfMz, sizeof(double), 1, m_fp);
				fread(&nCharge, sizeof(int), 1, m_fp);
			}
			delete[] lfPeaks;
		}
		if (m_fp != NULL) {
			fclose(m_fp);
			m_fp = NULL;
		}

		// open the file to be read
		if (m_fp == NULL)
			m_fp = fopen(strPath.c_str(), "rb");
		fread(&m_tTotalScan, sizeof(int), 1, m_fp);

		nSize = 0;
		fread(&nSize, sizeof(int), 1, m_fp);

		szTitle = new char[nSize + 1];
		fread(szTitle, sizeof(char), nSize, m_fp);
		szTitle[nSize] = '\0';

		m_strTitle = szTitle;
		delete[] szTitle;
	}
	catch (exception & e) {
		CErrInfo info("PFInput", "startLoad", "OpenInFile: " + strPath + " failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("PFInput", "startLoad", "caught an unknown exception from OpenInFile: " + strPath + " failed.");
		throw runtime_error(info.Get().c_str());
	}
	return;
}

void PFInput::loadNext(SPECTRUM &stSpec, int &nIdx)
{
	try {
		int nScan = 0;
		fread(&nScan, sizeof(int), 1, m_fp);
		stSpec.m_nScanNo = nScan;

		stSpec.vPeaksTbl.clear();
		fread(&stSpec.nPeaksNum, sizeof(int), 1, m_fp);
		stSpec.vPeaksTbl.reserve(stSpec.nPeaksNum);
		double *lfPeaks = new double[stSpec.nPeaksNum << 1];
		fread(lfPeaks, sizeof(double), stSpec.nPeaksNum << 1, m_fp);
			
		for (int i = 0; i < stSpec.nPeaksNum; ++i) {
			FRAGMENT_ION stPeak(lfPeaks[i << 1], lfPeaks[(i << 1) + 1]);
			stSpec.vPeaksTbl.push_back(stPeak);
		}

		int nPreNum = 0;
		fread(&nPreNum, sizeof(int), 1, m_fp);
		stSpec.nPrecurNum = nPreNum;
		double lfMz = 0;
		int nCharge = 0;
		for (int j = 0; j < nPreNum; ++j){
			fread(&lfMz, sizeof(double), 1, m_fp);
			fread(&nCharge, sizeof(int), 1, m_fp);
			PRECURSOR precur(nCharge, (lfMz-IonMass_Proton)*nCharge + IonMass_Proton, 0);
			stSpec.vPrecursors.push_back(precur);
		}
		// filename.scan.scan
		ostringstream oss;
		oss << m_strTitle << "."
			<< stSpec.m_nScanNo << "."
			<< stSpec.m_nScanNo;
		stSpec.strSpecTitle = oss.str();

		delete[] lfPeaks;

		++ nIdx;
	}
	catch (exception & e) {
		CErrInfo info("PFInput", "loadNext", "fail to load next spectrum");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("PFInput", "loadNext", "caught an unknown exception.");
		throw runtime_error(info.Get().c_str());
	}
}

void PFInput::loadNextBatch(int nNumber, std::vector<SPECTRUM> &vSpec, int &nIdx)
{
	vSpec.clear();

	for (int i = 0; i < nNumber; ++i) {
		SPECTRUM spec;
		loadNext(spec, nIdx);
		vSpec.push_back(spec);
	}
}

void PFInput::loadPrev(SPECTRUM &stSpec, int &nIdx)
{

}


void PFInput::loadSpec(SPECTRUM &stSpec, const std::string &strTitle, int &nSpecNo)
{

}

void PFInput::endLoad()
{
	if (m_fp != NULL) {
		fclose(m_fp);
		m_fp = NULL;
	}
}

int PFInput::GetScanNum() const
{
	return m_tTotalScan;
}
int PFInput:: GetSpecNum() const
{
	return m_tTotalSpec;
}

/*end: class PFInput*/

