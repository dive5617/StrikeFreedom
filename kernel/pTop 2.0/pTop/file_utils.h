#ifndef FILE_UTILES_H
#define FILE_UTILES_H

#include <iostream>
#include <cstdio>
#include <dirent.h>
#include <unordered_set>
#include "sdk.h"
#include "util.h"
#include "config.h"




using namespace std;

class MS2Input;
class MS2InputFactory{
public:
	MS2InputFactory();
	virtual ~MS2InputFactory();

	MS2Input* getImporter(MS2FormatType eType);
};

// abstract class
class MS2Input{
public:
	MS2Input(){}
	virtual ~MS2Input(){}

	virtual MS2FormatType getType() = 0;
	virtual void loadAll(const std::string &strPath,std::vector<SPECTRUM> &vSpec) = 0;
	virtual void startLoad(const std::string &strPath) = 0;
	virtual void loadNext(SPECTRUM &stSpec, int &nIdx) = 0;
	virtual void loadPrev(SPECTRUM &stSpec, int &nIdx) = 0;
	virtual void loadNextBatch(int nNumber, std::vector<SPECTRUM> &vSpec, int &nIdx) = 0;
	virtual void loadSpec(SPECTRUM &stSpec, const std::string &strTitle, int &nSpecNo) = 0;
	virtual void endLoad() = 0;   // close file stream
	virtual int GetScanNum() const = 0;
	virtual int GetSpecNum() const = 0;
};

class MGFInput : public MS2Input {
	std::string m_strFilePath;
	std::ifstream m_stIn;
	//std::vector<size_t> m_vSpecLineNum;
	std::vector<size_t> m_vPeakNum;  // peak number of each spectra
	char m_cSeperator;
	size_t m_tTotalScan;
	size_t m_tTotalSpec;
	size_t m_tCurSpec;
	size_t m_tCurLine;

public:
	MGFInput();
	virtual ~MGFInput();

	virtual MS2FormatType getType() {
		return MFT_MGF;
	}
	virtual void loadAll(const std::string &strPath, std::vector<SPECTRUM> &vSpec);
	virtual void startLoad(const std::string &strPath);
	virtual void loadNext(SPECTRUM &stSpec, int &nIdx);
	virtual void loadPrev(SPECTRUM &stSpec, int &nIdx);
	virtual void loadNextBatch(int nNumber, std::vector<SPECTRUM> &vSpec, int &nIdx);
	virtual void loadSpec(SPECTRUM &stSpec, const std::string &strTitle, int &nSpecNo);
	virtual void endLoad();
	int GetScanNum() const;
	int GetSpecNum() const;

protected:
	void openInFile(const std::string &strFilePath);
	void closeInFile();
	size_t checkLegalityAndCount();
};

class PFInput : public MS2Input {
protected:
	FILE *m_fp;
	std::string m_strTitle;
	size_t m_tTotalScan;
	size_t m_tTotalSpec;


public:
	PFInput();
	virtual ~PFInput();

	virtual MS2FormatType getType() {
		return MFT_PF;
	}
	virtual void loadAll(const std::string &strPath, std::vector<SPECTRUM> &vSpec);
	virtual void startLoad(const std::string &strPath); //, FILE *pf
	virtual void loadNext(SPECTRUM &stSpec, int &nIdx);
	virtual void loadPrev(SPECTRUM &stSpec, int &nIdx);
	virtual void loadNextBatch(int nNumber, std::vector<SPECTRUM> &vSpec, int &nIdx);
	virtual void loadSpec(SPECTRUM &stSpec, const std::string &strTitle, int &nSpecNo);
	virtual void endLoad();
	int GetScanNum() const;
	int GetSpecNum() const;
};


#endif