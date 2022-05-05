defaultHyperparamsDictionary = {"a":2,
                                "b":1923142,
                                "c":4,
                                "d":10}


def main():
    resultDict = konstruerHyperparamsDictionary(d=-233232, b=0)
    
    resultHyperparamsList = list(resultDict.values())
    
    print("resultHyperparamsList: ", resultHyperparamsList)
    
    
def konstruerHyperparamsDictionary(**nonDefaultKWArgs):
    hyperparamsDictionary = defaultHyperparamsDictionary
    
    for nonDefaultArgKey in nonDefaultKWArgs.keys():
        hyperparamsDictionary[str(nonDefaultArgKey)]=nonDefaultKWArgs[str(nonDefaultArgKey)]
    
    return hyperparamsDictionary
    
    
if __name__ == "__main__":
    main()



























# def min_sum(*bruhs):
    # summ = 0
    # for heltall in bruhs: # bruhs er en tuppel
        # summ += heltall
        
    # return summ

# def print_min_setning(**kwargs):
    # result = ""
    
    # for verdi in kwargs.values():
        # result += verdi + " "

    # print("Hær ær resultatet: " + result)
    