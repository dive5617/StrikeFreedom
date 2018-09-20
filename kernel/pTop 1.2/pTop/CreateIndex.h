/*
 *  CreateIndex.h
 *
 *  Created on: 2013-10-28
 *  Author: luolan
 */

#ifndef CREATEINDEX_H
#define CREATEINDEX_H

#include <iostream>
#include <cstring>
#include <fstream>
#include <vector>

#include "predefine.h"
#include "MapAAMass.h"
#include "BasicTools.h"

using namespace std;

class CCreateIndex
{
private:
	int m_ProteinNum;        // Total number of proteins
	long int m_nProTotalLen; // Total length of all proteins, to determine the size of TAG table
    string m_DBFile;         // Path and name of the database file
	ifstream m_fpro;
	CMapAAMass *mapProMass;
	Clog *m_clog;
	
	void _Trim(string &str);

public:
    vector<PROTEIN_STRUCT> m_ProteinList; // All peotein should sort by mass

    CCreateIndex(const string &strDBFile, CMapAAMass *mapMass);
    ~CCreateIndex();
    bool IndexofProtein();                // Get all protein info and sort by mass
	int GetProteinNum();
	long int GetProTotalLen();

};
#endif
