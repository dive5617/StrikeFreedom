/*
 *  Quantitation.h
 *
 *  Created on: 2016-02-29
 *  Author: wang ruimin
 */

#ifndef QUANTIFY_H
#define QUANTIFY_H

#include <unordered_map>
#include <cstring>
#include <algorithm>
#include "../sdk.h"
#include "../util.h"
#include "../config.h"

using namespace std;


class Quantitation
{
public:
	Quantitation(CConfiguration *m_cPara);
	~Quantitation();
	void getLabelbyName(string name,LABEL_QUANT &label_info);

	void quantify();

private:
	void rewriteQuantCfg(const string &cfgPath, int fileIdx);
	void callpQuant(const string &cfgPath);
	string getMS1bySpecFile(const string &infile, string &ms1Type);

	CConfiguration *m_cPara;
	Clog *m_pTrace;
};

#endif
